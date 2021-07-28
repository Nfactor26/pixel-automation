using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Core.Notfications;
using Pixel.Automation.TestExplorer.ViewModels;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Pixel.Automation.TestExplorer
{
    /// <summary>
    /// TestCaseManager manages test cases for active Automation Project 
    /// </summary>
    public class TestRepositoryManager : PropertyChangedBase, IHandle<TestCaseUpdatedEventArgs>,
        IHandle<TestFilterNotification>, ITestRepositoryManager
    {
        #region Data members
    
        private readonly ILogger logger = Log.ForContext<TestRepositoryManager>();

        private readonly IProjectFileSystem fileSystem;      
        private readonly IProjectManager projectManager;       
        private readonly IEventAggregator eventAggregator;      
        private readonly IWindowManager windowManager;
        private readonly ApplicationSettings applicationSettings;
        private bool isInitialized = false;

        public ITestRunner TestRunner { get; }

        public BindableCollection<TestFixtureViewModel> TestFixtures { get; set; } = new BindableCollection<TestFixtureViewModel>();
      
        string filterText = string.Empty;
        public string FilterText
        {
            get
            {
                return filterText;
            }
            set
            {
                filterText = value;
                var fixtureView = CollectionViewSource.GetDefaultView(TestFixtures);
                fixtureView.Refresh();             
                NotifyOfPropertyChange(() => FilterText);
            }
        }

        #endregion Data members

        #region Constructor

        public TestRepositoryManager(IEventAggregator eventAggregator, IAutomationProjectManager projectManager, IProjectFileSystem fileSystem, ITestRunner testRunner, IWindowManager windowManager, ApplicationSettings applicationSettings)
        {
            this.projectManager = Guard.Argument(projectManager).NotNull().Value;
            this.fileSystem = Guard.Argument(fileSystem).NotNull().Value;
            this.TestRunner = Guard.Argument(testRunner).NotNull().Value;        
            this.eventAggregator = Guard.Argument(eventAggregator).NotNull().Value;            
            this.windowManager = Guard.Argument(windowManager).NotNull().Value;
            this.applicationSettings = Guard.Argument(applicationSettings).NotNull();
            this.eventAggregator.SubscribeOnPublishedThread(this);
           
            CreateDefaultView();           
        }

        public void Initialize()
        {
            if (!isInitialized)
            {
                foreach (var testFixtureDirectory in Directory.GetDirectories(this.fileSystem.TestCaseRepository))
                {
                    var testFixture = this.fileSystem.LoadFiles<TestFixture>(testFixtureDirectory).Single();
                    TestFixtureViewModel testFixtureVM = new TestFixtureViewModel(testFixture);
                    this.TestFixtures.Add(testFixtureVM);

                    foreach (var testCase in this.fileSystem.LoadFiles<TestCase>(testFixtureDirectory))
                    {
                        TestCaseViewModel testCaseVM = new TestCaseViewModel(testCase, this.eventAggregator);                      
                        testFixtureVM.Tests.Add(testCaseVM);
                    }
                }
                isInitialized = true;
            }            
        }

        private void CreateDefaultView()
        {
            var fixtureGroupedItems = CollectionViewSource.GetDefaultView(TestFixtures);
            fixtureGroupedItems.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            fixtureGroupedItems.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Ascending));
            fixtureGroupedItems.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));
            fixtureGroupedItems.Filter = new Predicate<object>((a) =>
            {
                if (a is TestFixtureViewModel fixture)
                {
                    fixture.UpdateVisibility(filterText);                  
                    return fixture.IsVisible;
                }
                return true;
            });
        }

        #endregion Constructor

        #region Test Fixture

        public bool CanAddTestFixture
        {
            get => true;
        }

        public async Task AddTestFixtureAsync()
        {
            try
            {
                TestFixture newTestFixture = new TestFixture()
                {
                    DisplayName = $"Fixture#{TestFixtures.Count() + 1}",
                    Order = TestFixtures.Count() + 1,
                    TestFixtureEntity = new TestFixtureEntity()
                };
                TestFixtureViewModel testFixtureVM = new TestFixtureViewModel(newTestFixture)
                {
                    DelayFactor = applicationSettings.DelayFactor
                };
                var testCategoryEditor = new EditTestFixtureViewModel(testFixtureVM, this.TestFixtures);
                bool? result = await this.windowManager.ShowDialogAsync(testCategoryEditor);
                if (result.HasValue && result.Value)
                {
                    this.TestFixtures.Add(testFixtureVM);
                    SaveTestFixture(testFixtureVM);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public async Task EditTestFixtureAsync(TestFixtureViewModel fixtureVM)
        {
            try
            {
                var testCategoryEditor = new EditTestFixtureViewModel(fixtureVM, this.TestFixtures.Except(new[] { fixtureVM }));
                bool? result = await this.windowManager.ShowDialogAsync(testCategoryEditor);
                if (result.HasValue && result.Value)
                {
                    SaveTestFixture(fixtureVM, false);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public async Task OpenTestFixtureAsync(TestFixtureViewModel fixtureVM)
        {
            try
            {
                Guard.Argument(fixtureVM).NotNull();

                logger.Information("Trying to open test fixture : {0} for edit.", fixtureVM.DisplayName);

                if (!fixtureVM.CanOpenForEdit)
                {
                    logger.Information($"Test fixture : {fixtureVM.DisplayName} is alreayd open for edit.");
                    return;
                }
                     
                Dispatcher dispatcher = System.Windows.Application.Current.Dispatcher;
                if (!dispatcher.CheckAccess())
                {
                    System.Action openForEditAction = async () => await OpenTestFixtureAsync(fixtureVM);
                    dispatcher.Invoke(openForEditAction);
                    return;
                }
                ITestCaseFileSystem testCaseFileSystem = this.fileSystem.CreateTestCaseFileSystemFor(fixtureVM.Id);
                fixtureVM.TestFixtureEntity = this.projectManager.Load<Entity>(testCaseFileSystem.FixtureProcessFile);
                fixtureVM.TestFixtureEntity.Name = fixtureVM.DisplayName;
                fixtureVM.TestFixtureEntity.Tag = fixtureVM.Id;
                             
                if (await this.TestRunner.TryOpenTestFixture(fixtureVM.TestFixture))
                {
                    SetupScriptEditor();                
                    fixtureVM.IsOpenForEdit = true;
                    NotifyOfPropertyChange(nameof(CanSaveAll));
                }

                void SetupScriptEditor()
                {
                    var fixtureEntityManager = fixtureVM.TestFixtureEntity.EntityManager;
                    IScriptEditorFactory editorFactory = fixtureEntityManager.GetServiceOfType<IScriptEditorFactory>();
                    editorFactory.AddProject(fixtureVM.Id, Array.Empty<string>(), typeof(Empty));
                    string fixtureScriptContent = File.ReadAllText(testCaseFileSystem.FixtureScript);
                    editorFactory.AddDocument(fixtureVM.ScriptFile, fixtureVM.Id, fixtureScriptContent);
                }
                logger.Information($"Test fixture : {fixtureVM.DisplayName} is open for edit now.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public async Task OpenTestFixtureAsync(string fixtureId)
        {
            try
            {
                var fixtureToOpen = this.TestFixtures.FirstOrDefault(f => f.Id.Equals(fixtureId));
                if (fixtureToOpen != null)
                {
                    await OpenTestFixtureAsync(fixtureToOpen);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public async Task CloseTestFixtureAsync(TestFixtureViewModel fixtureVM, bool autoSave)
        {
            if (fixtureVM.IsOpenForEdit)
            {
                logger.Information($"Trying to close test fixture : {fixtureVM.DisplayName} for edit.");

                if (autoSave)
                {
                    SaveTestFixture(fixtureVM);
                }          
                
                foreach(var testCase in fixtureVM.Tests)
                {
                    await CloseTestCaseAsync(testCase, autoSave);
                }

                var fixtureEntityManager = fixtureVM.TestFixtureEntity.EntityManager;
                IScriptEditorFactory scriptEditorFactory = fixtureEntityManager.GetServiceOfType<IScriptEditorFactory>();
                scriptEditorFactory.RemoveProject(fixtureVM.Id);

                await this.TestRunner.TryCloseTestFixture(fixtureVM.TestFixture);
               
                fixtureVM.TestFixtureEntity = null;
                fixtureVM.IsOpenForEdit = false;
                logger.Information($"Test fixture {fixtureVM.DisplayName} has been closed.");

            }
            NotifyOfPropertyChange(nameof(CanSaveAll));
        }

        public async Task CloseTestFixtureAsync(string fixtureId)
        {
            try
            {
                var fixtureToOpen = this.TestFixtures.FirstOrDefault(f => f.Id.Equals(fixtureId));
                if (fixtureToOpen != null)
                {
                    await CloseTestFixtureAsync(fixtureToOpen, true);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public async Task EditTestFixtureScriptAsync(TestFixtureViewModel fixtureVM)
        {
            try
            {
                ITestCaseFileSystem testCaseFileSystem = this.fileSystem.CreateTestCaseFileSystemFor(fixtureVM.Id);
                var fixtureEntityManager = fixtureVM.TestFixtureEntity.EntityManager;
                IScriptEditorFactory scriptEditorFactory = fixtureEntityManager.GetServiceOfType<IScriptEditorFactory>();              
                using (IScriptEditorScreen scriptEditorScreen = scriptEditorFactory.CreateScriptEditor())
                {
                    scriptEditorScreen.OpenDocument(fixtureVM.ScriptFile, fixtureVM.Id, string.Empty);
                    var result = await this.windowManager.ShowDialogAsync(scriptEditorScreen);

                    if (result.HasValue && result.Value)
                    {
                        if (fixtureVM.IsOpenForEdit)
                        {
                            var fixtureScriptEngine = fixtureVM.TestFixtureEntity.EntityManager.GetScriptEngine();
                            fixtureScriptEngine.ClearState();
                            await fixtureScriptEngine.ExecuteFileAsync(fixtureVM.ScriptFile);

                            foreach (var testCaseVM in fixtureVM.Tests)
                            {
                                if (testCaseVM.IsOpenForEdit)
                                {
                                    var scriptEngine = testCaseVM.TestCaseEntity.EntityManager.GetScriptEngine();
                                    scriptEngine.ClearState();
                                    await scriptEngine.ExecuteFileAsync(fixtureVM.ScriptFile);
                                    await scriptEngine.ExecuteFileAsync(testCaseVM.ScriptFile);
                                }
                            }
                        }

                    }
                }               
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }
      
        public void SaveTestFixture(TestFixtureViewModel testFixtureVM, bool saveFixtureEntity = true)
        {
            try
            {
                ITestCaseFileSystem testCaseFileSystem = this.fileSystem.CreateTestCaseFileSystemFor(testFixtureVM.Id);
                if (string.IsNullOrEmpty(testFixtureVM.ScriptFile))
                {
                    testFixtureVM.ScriptFile = testCaseFileSystem.GetRelativePath(testCaseFileSystem.FixtureScript);
                    testCaseFileSystem.CreateOrReplaceFile(testCaseFileSystem.FixtureDirectory, Path.GetFileName(testFixtureVM.ScriptFile), string.Empty);
                }

                testCaseFileSystem.SaveToFile<TestFixture>(testFixtureVM.TestFixture, testCaseFileSystem.FixtureDirectory, Path.GetFileName(testCaseFileSystem.FixtureFile));
                if (saveFixtureEntity)
                {
                    //Remove the test case entities before saving test fixture entity
                    var testCaseEntities = testFixtureVM.TestFixtureEntity.GetComponentsOfType<TestCaseEntity>(SearchScope.Descendants);
                    foreach (var testEntity in testCaseEntities)
                    {
                        testEntity.Parent.RemoveComponent(testEntity);
                    }

                    testCaseFileSystem.SaveToFile<Entity>(testFixtureVM.TestFixtureEntity, testCaseFileSystem.FixtureDirectory, Path.GetFileName(testCaseFileSystem.FixtureProcessFile));

                    //Add back the test cases that were removed earlier
                    foreach (var testEntity in testCaseEntities)
                    {
                        testFixtureVM.TestFixtureEntity.AddComponent(testEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public void DeleteTestFixture(TestFixtureViewModel testFixtureVM)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this test fixture along with all tests?", "Confirm Delete", MessageBoxButton.OKCancel);
            if(result == MessageBoxResult.OK)
            {
                this.TestFixtures.Remove(testFixtureVM);
                Directory.Delete(Path.Combine(this.fileSystem.TestCaseRepository, testFixtureVM.Id));
            }
        }       

        #endregion Test Fixture

        #region Test Case      

        public async Task AddTestCaseAsync(TestFixtureViewModel testFixtureVM)
        {
            try
            {
                TestCase testCase = new TestCase()
                {
                    FixtureId = testFixtureVM.Id,
                    DisplayName = $"Test#{testFixtureVM.Tests.Count() + 1}",
                    Order = testFixtureVM.Tests.Count() + 1,
                    TestCaseEntity = new TestCaseEntity() { Name = $"Test#{testFixtureVM.Tests.Count() + 1}" }
                };
                TestCaseViewModel testCaseVM = new TestCaseViewModel(testCase, this.eventAggregator)
                {
                    DelayFactor = testFixtureVM.DelayFactor
                };
             
                var testCaseEditor = new EditTestCaseViewModel(testCaseVM, testFixtureVM.Tests);               
                bool? result = await this.windowManager.ShowDialogAsync(testCaseEditor);
                if (result.HasValue && result.Value)
                {
                    testFixtureVM.Tests.Add(testCaseVM);
                    SaveTestCase(testCaseVM);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public async Task EditTestCaseAsync(TestCaseViewModel testCaseVM)
        {
            try
            {
                var parentFixture = TestFixtures.First(t => t.Id.Equals(testCaseVM.FixtureId));               
                var existingTestCases = parentFixture.Tests.Except(new[] { testCaseVM });
                var testCaseEditor = new EditTestCaseViewModel(testCaseVM, existingTestCases);
                bool? result = await this.windowManager.ShowDialogAsync(testCaseEditor);
                if (result.HasValue && result.Value)
                {
                    if (testCaseVM.IsOpenForEdit)
                    {
                        testCaseVM.TestCaseEntity.Name = testCaseVM.DisplayName;
                    }
                    SaveTestCase(testCaseVM, false);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public void SaveTestCase(TestCaseViewModel testCaseVM, bool saveTestEntity = true)
        {
            ITestCaseFileSystem testCaseFileSystem = this.fileSystem.CreateTestCaseFileSystemFor(testCaseVM.FixtureId);
            TestFixtureViewModel testFixtureVM = this.TestFixtures.FirstOrDefault(c => c.Id.Equals(testCaseVM.FixtureId));
            if(testFixtureVM != null)
            {                
                if (string.IsNullOrEmpty(testCaseVM.ScriptFile))
                {
                    testCaseVM.ScriptFile = testCaseFileSystem.GetRelativePath(testCaseFileSystem.GetTestScriptFile(testCaseVM.Id));
                    testCaseFileSystem.CreateOrReplaceFile(testCaseFileSystem.FixtureDirectory, testCaseFileSystem.GetTestScriptFile(testCaseVM.Id), string.Empty);
                }

                var prefabsUsed = testCaseVM.TestCaseEntity.GetComponentsOfType<PrefabEntity>(SearchScope.Descendants);
                if(prefabsUsed.Any())
                {
                    testCaseVM.PrefabReferences = new Core.Models.PrefabReferences();
                    foreach (var prefab in prefabsUsed)
                    {
                        testCaseVM.PrefabReferences.AddPrefabReference(new Core.Models.PrefabReference(prefab.ApplicationId, prefab.PrefabId));
                    }
                }

                testCaseFileSystem.SaveToFile<TestCase>(testCaseVM.TestCase, testCaseFileSystem.FixtureDirectory);               
                if(saveTestEntity)
                {
                    testCaseFileSystem.SaveToFile<Entity>(testCaseVM.TestCaseEntity, testCaseFileSystem.FixtureDirectory, testCaseFileSystem.GetTestProcessFile(testCaseVM.Id));               
                }           
              
            }
        }

        public void DeleteTestCase(TestCaseViewModel testCaseVM)
        {
            if (testCaseVM.IsOpenForEdit)
            {
                MessageBox.Show("Can't delete test case. Test case is open for edit.", "Confirm Delete", MessageBoxButton.OK);
                return;
            }

            TestFixtureViewModel testFixtureVM = this.TestFixtures.FirstOrDefault(c => c.Id.Equals(testCaseVM.FixtureId));
            if (testFixtureVM != null)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this test", "Confirm Delete", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    ITestCaseFileSystem testCaseFileSystem = this.fileSystem.CreateTestCaseFileSystemFor(testCaseVM.FixtureId);
                    testCaseFileSystem.DeleteTestCase(testCaseVM.Id);
                    testFixtureVM.Tests.Remove(testCaseVM);
                    logger.Information($"Deleted test case : {testCaseVM.DisplayName} from fixture : {testFixtureVM.DisplayName}");
                }              
            }
        }      

        public async Task OpenTestCaseAsync(TestCaseViewModel testCaseVM)
        {
            try
            {
                logger.Information($"Trying to open test case : {testCaseVM.DisplayName} for edit.");

                if (!testCaseVM.CanOpenForEdit)
                {
                    logger.Information($"Test case : {testCaseVM.DisplayName} is alreayd open for edit.");
                    return;
                }

                //This is required because OpenForEdit might be called from RunAll for tests which are not open for edit.
                //Opening a test initializes dependencies like script engine , script editor , etc for test case.
                //script editor can only be created on dispatcher thread although it won't be used while running.          
                Dispatcher dispatcher = System.Windows.Application.Current.Dispatcher;
                if (!dispatcher.CheckAccess())
                {
                    System.Action openForEditAction = async () => await OpenTestCaseAsync(testCaseVM);
                    dispatcher.Invoke(openForEditAction);
                    return;
                }

                ITestCaseFileSystem testCaseFileSystem = this.fileSystem.CreateTestCaseFileSystemFor(testCaseVM.FixtureId);
                string testCaseProcessFile = testCaseFileSystem.GetTestProcessFile(testCaseVM.Id);
                testCaseVM.TestCaseEntity = this.projectManager.Load<Entity>(testCaseProcessFile);
                testCaseVM.TestCaseEntity.Name = testCaseVM.DisplayName;
                testCaseVM.TestCaseEntity.Tag = testCaseVM.Id;

                var parentFixture = this.TestFixtures.First(f => f.Id.Equals(testCaseVM.FixtureId));
                if(!parentFixture.IsOpenForEdit)
                {
                    await this.OpenTestFixtureAsync(parentFixture);
                }

                if (await this.TestRunner.TryOpenTestCase(parentFixture.TestFixture, testCaseVM.TestCase))
                {
                    SetupScriptEditor();                  
                    testCaseVM.IsOpenForEdit = true;
                    NotifyOfPropertyChange(nameof(CanSaveAll));
                }

                void SetupScriptEditor()
                {
                    var testEntityManager = testCaseVM.TestCase.TestCaseEntity.EntityManager;
                    IScriptEditorFactory editorFactory = testEntityManager.GetServiceOfType<IScriptEditorFactory>();
                                 
                    //Add the test script project and script file to script editor
                    editorFactory.AddProject(testCaseVM.Id, new string[] { parentFixture.Id }, testEntityManager.Arguments.GetType());
                    string scriptFileContent = File.ReadAllText(testCaseFileSystem.GetTestScriptFile(testCaseVM.Id));
                    editorFactory.AddDocument(testCaseVM.ScriptFile, testCaseVM.Id, scriptFileContent);
                }
                logger.Information($"Test Case : {testCaseVM.DisplayName} is open for edit now.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }

        }

        public async Task OpenTestCaseAsync(string testCaseId)
        {
            try
            {
                foreach (var testFixture in this.TestFixtures)
                {
                    var targetTestCase = testFixture.Tests.FirstOrDefault(t => t.Id.Equals(testCaseId));
                    if (targetTestCase != null)
                    {
                        await OpenTestCaseAsync(targetTestCase);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public async Task CloseTestCaseAsync(TestCaseViewModel testCaseVM, bool autoSave)
        {
            try
            {
                if (testCaseVM.IsOpenForEdit)
                {
                    logger.Information($"Trying to close test case : {testCaseVM.DisplayName} for edit.");

                    if (autoSave)
                    {
                        SaveTestCase(testCaseVM);
                    }                 
                    var parentFixture = this.TestFixtures.First(f => f.Id.Equals(testCaseVM.FixtureId));

                    //Remove the script editor project that was added while opening the test case
                    var entityManager = testCaseVM.TestCaseEntity.EntityManager;
                    var scriptEditorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
                    scriptEditorFactory.RemoveProject(testCaseVM.Id);

                    await this.TestRunner.TryCloseTestCase(parentFixture.TestFixture, testCaseVM.TestCase);                 

                    testCaseVM.TestCaseEntity = null;
                    testCaseVM.IsOpenForEdit = false;
                    logger.Information($"Test Case {testCaseVM.DisplayName} has been closed.");

                }
                NotifyOfPropertyChange(nameof(CanSaveAll));
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public async Task CloseTestCaseAsync(string testCaseId)
        {
            try
            {
                foreach (var fixture in this.TestFixtures)
                {
                    if (!fixture.IsOpenForEdit)
                    {
                        continue;
                    }
                    foreach (var test in fixture.Tests)
                    {
                        if (test.Id.Equals(testCaseId))
                        {
                            await CloseTestCaseAsync(test, true);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }      
       
        public async Task EditTestScriptAsync(TestCaseViewModel testCaseVM)
        {
            try
            {
                if (testCaseVM.IsOpenForEdit)
                {
                    var entityManager = testCaseVM.TestCaseEntity.EntityManager;
                    var scriptEditorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();                  
                    using (IScriptEditorScreen scriptEditorScreen = scriptEditorFactory.CreateScriptEditor())
                    {
                        scriptEditorScreen.OpenDocument(testCaseVM.ScriptFile, testCaseVM.Id, string.Empty);
                        var result = await this.windowManager.ShowDialogAsync(scriptEditorScreen);
                        if (result.HasValue && result.Value)
                        {
                            var scriptEngine = entityManager.GetScriptEngine();
                            scriptEngine.ClearState();
                            var parentFixture = this.TestFixtures.First(f => f.Id.Equals(testCaseVM.FixtureId));
                            await scriptEngine.ExecuteFileAsync(parentFixture.ScriptFile);
                            await scriptEngine.ExecuteFileAsync(testCaseVM.ScriptFile);
                        }
                    }   
                    
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
         
        }

        public async void ShowDataSource(TestCaseViewModel testCaseVM)
        {
            await this.eventAggregator.PublishOnUIThreadAsync(new ShowTestDataSourceNotification(testCaseVM.TestDataId));
        }

        #endregion Test Case

        #region save all

        public bool CanSaveAll
        {
            get => this.TestFixtures.Any(a => a.IsOpenForEdit);
        }

        /// <summary>
        /// Save all open test fixtures and test cases
        /// </summary>
        public void SaveAll()
        {
            foreach (var fixture in this.TestFixtures)
            {
                try
                {
                    if(fixture.IsOpenForEdit)
                    {
                        SaveTestFixture(fixture, true);
                        foreach (var testCase in fixture.Tests)
                        {
                            if (testCase.IsOpenForEdit)
                            {
                                SaveTestCase(testCase, true);
                            }
                        }
                    }                    
                }
                catch (Exception ex)
                {
                    logger.Error(ex, ex.Message);
                }
            }
        }

        #endregion save all

        #region Execute

        bool isCancellationRequested = false;

        bool canAbort = false;
        public bool CanAbort
        {
            get => canAbort;
            set
            {
                canAbort = value;
                NotifyOfPropertyChange();
            }
        }

        public void RunSelected()
        {
            Task runTestCasesTask = new Task(async () =>
            {
                foreach (var testFixture in this.TestFixtures)
                {                   
                    foreach (var test in testFixture.Tests)
                    {
                        if (test.IsSelected)
                        {
                            //open fixture if not already open
                            bool isFixtureAlreadyOpenForEdit = testFixture.IsOpenForEdit;
                            if (!testFixture.IsOpenForEdit)
                            {
                                await OpenTestFixtureAsync(testFixture);
                            }

                            await this.TestRunner.OneTimeSetUp(testFixture.TestFixture);
                            await TryRunTestCaseAsync(test);
                            await this.TestRunner.OneTimeTearDown(testFixture.TestFixture);
                           
                            //close fixture if it was opened
                            if (!isFixtureAlreadyOpenForEdit)
                            {
                                await CloseTestFixtureAsync(testFixture, false);
                            }

                            break;
                        }
                    }
                    
                }
            });
            runTestCasesTask.Start();
        }

        public void RunAll()
        {
            isCancellationRequested = false;
            CanAbort = true;
            Task runTestCasesTask = new Task(async() =>
            {
                try
                {
                    foreach (var testFixture in this.TestFixtures.OrderBy(t => t.Order).ThenBy(t => t.DisplayName))
                    {
                        //open fixture if not already open
                        bool isFixtureAlreadyOpenForEdit = testFixture.IsOpenForEdit;
                        if (!testFixture.IsOpenForEdit)
                        {
                            await OpenTestFixtureAsync(testFixture);
                        }

                        await this.TestRunner.OneTimeSetUp(testFixture.TestFixture);
                        foreach (var test in testFixture.Tests.OrderBy(t => t.Order).ThenBy(t => t.DisplayName))
                        {
                            if (!isCancellationRequested)
                            {
                                await TryRunTestCaseAsync(test);
                            }
                        }
                        await this.TestRunner.OneTimeTearDown(testFixture.TestFixture);

                        //close fixture if it was opened
                        if (!isFixtureAlreadyOpenForEdit)
                        {
                            await CloseTestFixtureAsync(testFixture, false);
                        }
                    }
                    CanAbort = false;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, ex.Message);
                }
            });           
            runTestCasesTask.Start();
        }

        public void AbortRun()
        {
            isCancellationRequested = true;
        }      

        private async Task<bool> TryRunTestCaseAsync(TestCaseViewModel testCaseVM)
        {
            bool isAlreadyOpenForEdit = testCaseVM.IsOpenForEdit;
            try
            {
                if (testCaseVM.IsMuted)
                {
                    return await Task.FromResult(false);
                }

                if (!testCaseVM.IsOpenForEdit)
                {
                   await OpenTestCaseAsync(testCaseVM);
                }
            
                Dispatcher dispatcher = System.Windows.Application.Current.Dispatcher;
                System.Action clearTestResults = () => testCaseVM.TestResults.Clear();
                if (!dispatcher.CheckAccess())
                {
                    dispatcher.Invoke(clearTestResults);
                }
                else
                {
                    clearTestResults();
                }
                var parentFixture = this.TestFixtures.First(f => f.Id.Equals(testCaseVM.FixtureId));

                await foreach (var result in this.TestRunner.RunTestAsync(parentFixture.TestFixture, testCaseVM.TestCase))
                {
                    System.Action addTestResult = () => testCaseVM.TestResults.Add(result);
                    if (!dispatcher.CheckAccess())
                    {
                        dispatcher.Invoke(addTestResult);
                    }
                    else
                    {
                        addTestResult();
                    }
                }

                return await Task.FromResult(true);

            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return await Task.FromResult(false);
            }
            finally
            {
                if (!isAlreadyOpenForEdit)
                {
                   await CloseTestCaseAsync(testCaseVM, false);
                }
                testCaseVM.IsRunning = false;
            }
        }

        #endregion Execute       

        #region Notifications

        /// <summary>
        /// This will be called whenever a new test data source is dragged on a test case.
        /// When this happens, TestDataModel property of test case is updated and we need to save back the details
        /// </summary>
        /// <param name="message"></param>
        public async Task HandleAsync(TestCaseUpdatedEventArgs message, CancellationToken cancellationToken)
        {
            if (message != null && message.ModifiedTestCase != null)
            {
                var targetTestCase = this.TestFixtures.SelectMany(c => c.Tests).FirstOrDefault(t => t.Id.Equals(message.ModifiedTestCase.Id));               
                this.SaveTestCase(targetTestCase, false);
            }
            await Task.CompletedTask;
        }

        public async Task HandleAsync(TestFilterNotification message, CancellationToken cancellationToken)
        {
            this.FilterText = message?.FilterText ?? string.Empty;
            await Task.CompletedTask;
        }

        #endregion Notifications
    }

}

