using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Core.Notfications;
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
using Pixel.Automation.Editor.Core.Helpers;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    /// <summary>
    /// TestExplorerViewModel holds the TestFixtures and TestCases belonging to an Automation Project
    /// and allows various operations such as adding, editing, deleting, etc. Any modifications are only locall stored.
    /// Changes are pushed to a persistent store only when project is saved. 
    /// It also allows to execute test cases and see results at design time.
    /// </summary>
    public class TestExplorerViewModel : Screen, ITestExplorer, IHandle<TestFilterNotification>
    {
        #region Data members

        private readonly ILogger logger = Log.ForContext<TestExplorerViewModel>();

        private readonly IProjectFileSystem fileSystem;
        private readonly IProjectManager projectManager;
        private readonly IEventAggregator eventAggregator;
        private readonly IWindowManager windowManager;
        private readonly IPlatformProvider platformProvider;
        private readonly ApplicationSettings applicationSettings;
        private bool isInitialized = false;

        public ITestRunner TestRunner { get; private set; }

        /// <summary>
        /// A collection of TestFixtures belonging to the Automation Project
        /// </summary>
        public BindableCollection<TestFixtureViewModel> TestFixtures { get; set; } = new BindableCollection<TestFixtureViewModel>();

        /// <summary>
        /// Controls visibility of test fixtures and test cases displayed on view
        /// </summary>
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

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="eventAggregator"></param>
        /// <param name="projectManager"></param>
        /// <param name="fileSystem"></param>
        /// <param name="testRunner"></param>
        /// <param name="windowManager"></param>
        /// <param name="applicationSettings"></param>
        public TestExplorerViewModel(IEventAggregator eventAggregator, IAutomationProjectManager projectManager, IProjectFileSystem fileSystem, ITestRunner testRunner, 
            IWindowManager windowManager, IPlatformProvider platformProvider, ApplicationSettings applicationSettings)
        {
            this.projectManager = Guard.Argument(projectManager).NotNull().Value;
            this.fileSystem = Guard.Argument(fileSystem).NotNull().Value;
            this.TestRunner = Guard.Argument(testRunner).NotNull().Value;
            this.eventAggregator = Guard.Argument(eventAggregator).NotNull().Value;
            this.windowManager = Guard.Argument(windowManager).NotNull().Value;
            this.platformProvider = Guard.Argument(platformProvider).NotNull().Value;
            this.applicationSettings = Guard.Argument(applicationSettings).NotNull();
            this.eventAggregator.SubscribeOnPublishedThread(this);

            CreateDefaultView();
        }

        /// <summary>
        /// Load the fixtures and test case from local storage
        /// </summary>
        void Initialize()
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
                        TestCaseViewModel testCaseVM = new TestCaseViewModel(testCase);
                        testFixtureVM.Tests.Add(testCaseVM);
                    }
                }
                isInitialized = true;
            }
        }

        /// <summary>
        /// Setup the collection view with grouping and sorting
        /// </summary>
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

        /// <summary>
        /// Guard method to check if a TestFixutre can be added
        /// </summary>
        public bool CanAddTestFixture
        {
            get => true;
        }

        /// <summary>
        /// Add a new Test Fixture
        /// </summary>
        /// <returns></returns>
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
                var testFixtureEditor = new EditTestFixtureViewModel(newTestFixture, this.TestFixtures.Select(s => s.DisplayName));
                bool? result = await this.windowManager.ShowDialogAsync(testFixtureEditor);
                if (result.HasValue && result.Value)
                {
                    this.TestFixtures.Add(testFixtureVM);
                    SaveTestFixture(testFixtureVM);
                    logger.Information("Added new fixture {0}", newTestFixture.DisplayName);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        /// <summary>
        /// Edit the details of an existing TestFixture
        /// </summary>
        /// <param name="fixtureVM"></param>
        /// <returns></returns>
        public async Task EditTestFixtureAsync(TestFixtureViewModel fixtureVM)
        {
            try
            {
                var existingFixtures = this.TestFixtures.Except(new[] { fixtureVM }).Select(f => f.DisplayName);
                var testFixtureEditor = new EditTestFixtureViewModel(fixtureVM.TestFixture, existingFixtures);
                bool? result = await this.windowManager.ShowDialogAsync(testFixtureEditor);
                if (result.HasValue && result.Value)
                {
                    SaveTestFixture(fixtureVM, false);
                    logger.Information("Edited fixture {0}", fixtureVM.DisplayName);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        /// <summary>
        /// Delete an existing TestFixture
        /// </summary>
        /// <param name="testFixtureVM"></param>
        public void DeleteTestFixture(TestFixtureViewModel testFixtureVM)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this test fixture along with all tests?", "Confirm Delete", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                this.TestFixtures.Remove(testFixtureVM);
                Directory.Delete(Path.Combine(this.fileSystem.TestCaseRepository, testFixtureVM.Id));
                logger.Information("TestFixture {0} was deleted from local storage.", testFixtureVM.DisplayName);

            }
        }

        /// <summary>
        /// Open a TestFixture for editing on automation editor
        /// </summary>
        /// <param name="fixtureVM"></param>
        /// <returns></returns>
        public async Task OpenTestFixtureAsync(TestFixtureViewModel fixtureVM)
        {
            try
            {
                Guard.Argument(fixtureVM).NotNull();
                
                if (!fixtureVM.CanOpenForEdit)
                {
                    logger.Information("Test fixture {0} is already open for edit.", fixtureVM.DisplayName);
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
                    string fixtureScriptContent = testCaseFileSystem.ReadAllText(testCaseFileSystem.FixtureScript);
                    editorFactory.AddDocument(fixtureVM.ScriptFile, fixtureVM.Id, fixtureScriptContent);
                }
                logger.Information("Test fixture {0} is open for edit now.", fixtureVM.DisplayName);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        /// <summary>
        /// Open a TestFixture for edit on automation editor given it's fixtureId
        /// </summary>
        /// <param name="fixtureId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Close a TestFixture which was previously opened for edit on automation editor
        /// </summary>
        /// <param name="fixtureVM"></param>
        /// <param name="autoSave"></param>
        /// <returns></returns>
        public async Task CloseTestFixtureAsync(TestFixtureViewModel fixtureVM, bool autoSave)
        {
            if (fixtureVM.IsOpenForEdit)
            {               
                if (autoSave)
                {
                    SaveTestFixture(fixtureVM);
                }

                foreach (var testCase in fixtureVM.Tests)
                {
                    await CloseTestCaseAsync(testCase, autoSave);
                }

                var fixtureEntityManager = fixtureVM.TestFixtureEntity.EntityManager;
                IScriptEditorFactory scriptEditorFactory = fixtureEntityManager.GetServiceOfType<IScriptEditorFactory>();
                scriptEditorFactory.RemoveProject(fixtureVM.Id);
                fixtureVM.TestFixtureEntity.DisposeEditors();

                await this.TestRunner.TryCloseTestFixture(fixtureVM.TestFixture);

                fixtureVM.TestFixtureEntity = null;
                fixtureVM.IsOpenForEdit = false;
                logger.Information("Test fixture {0} has been closed.", fixtureVM.DisplayName);

            }
            NotifyOfPropertyChange(nameof(CanSaveAll));
        }

        /// <summary>
        /// Close a TestFixture given it's fixtureId which was previously opened TestFixture for edit on automation editor
        /// </summary>
        /// <param name="fixtureId"></param>
        /// <returns></returns>
        public async Task CloseTestFixtureAsync(string fixtureId)
        {
            try
            {
                var fixtureToClose = this.TestFixtures.FirstOrDefault(f => f.Id.Equals(fixtureId));
                if (fixtureToClose != null)
                {
                    await CloseTestFixtureAsync(fixtureToClose, true);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        /// <summary>
        /// Edit the initialization script for a TestFixture
        /// </summary>
        /// <param name="fixtureVM"></param>
        /// <returns></returns>
        public async Task EditTestFixtureScriptAsync(TestFixtureViewModel fixtureVM)
        {
            try
            {
                if (fixtureVM.IsOpenForEdit)
                {
                    var fixtureEntityManager = fixtureVM.TestFixtureEntity.EntityManager;
                    IScriptEditorFactory scriptEditorFactory = fixtureEntityManager.GetServiceOfType<IScriptEditorFactory>();
                    using (IScriptEditorScreen scriptEditorScreen = scriptEditorFactory.CreateScriptEditor())
                    {
                        scriptEditorScreen.OpenDocument(fixtureVM.ScriptFile, fixtureVM.Id, string.Empty);
                        var result = await this.windowManager.ShowDialogAsync(scriptEditorScreen);
                        if (result.HasValue && result.Value)
                        {
                            logger.Information("TestFixture script {0} was edited.", fixtureVM.ScriptFile);
                            var fixtureScriptEngine = fixtureEntityManager.GetScriptEngine();
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

        /// <summary>
        /// Save the details of TestFixture locally.
        /// </summary>
        /// <param name="testFixtureVM"></param>
        /// <param name="saveFixtureEntity"></param>
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
                logger.Information("Changes to TestFixture {0} were saved to local storage.", testFixtureVM.DisplayName);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        #endregion Test Fixture

        #region Test Case      

        /// <summary>
        /// Add a new TestCase to specified TestFixture
        /// </summary>
        /// <param name="testFixtureVM"></param>
        /// <returns></returns>
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
                
                var testCaseEditor = new EditTestCaseViewModel(testCase, testFixtureVM.Tests.Select(s => s.DisplayName));
                bool? result = await this.windowManager.ShowDialogAsync(testCaseEditor);
                if (result.HasValue && result.Value)
                {
                    TestCaseViewModel testCaseVM = new TestCaseViewModel(testCase)
                    {
                        DelayFactor = testFixtureVM.DelayFactor
                    };

                    testFixtureVM.Tests.Add(testCaseVM);
                    SaveTestCase(testCaseVM);
                    logger.Information("Added new TestCase {0} to Fixture {1}", testCase.DisplayName, testFixtureVM.DisplayName);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        /// <summary>
        /// Edit the details of a TestCase
        /// </summary>
        /// <param name="testCaseVM"></param>
        /// <returns></returns>
        public async Task EditTestCaseAsync(TestCaseViewModel testCaseVM)
        {
            try
            {
                var parentFixture = TestFixtures.First(t => t.Id.Equals(testCaseVM.FixtureId));
                var existingTestCases = parentFixture.Tests.Except(new[] { testCaseVM }).Select(s => s.DisplayName);
                var testCaseEditor = new EditTestCaseViewModel(testCaseVM.TestCase, existingTestCases);
                bool? result = await this.windowManager.ShowDialogAsync(testCaseEditor);
                if (result.HasValue && result.Value)
                {
                    if (testCaseVM.IsOpenForEdit)
                    {
                        testCaseVM.TestCaseEntity.Name = testCaseVM.DisplayName;
                    }
                    SaveTestCase(testCaseVM, false);
                    logger.Information("Edited TestCase {0}", testCaseVM.DisplayName);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        /// <summary>
        /// Delete an existing TestCase
        /// </summary>
        /// <param name="testCaseVM"></param>
        public void DeleteTestCase(TestCaseViewModel testCaseVM)
        {
            try
            {
                if (testCaseVM.IsOpenForEdit)
                {
                    MessageBox.Show("Test case is open for edit. An open test case can't be deleted.", "Delete Test Case", MessageBoxButton.OK);
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
                        logger.Information("Deleted TestCase {0} from fixture {1}", testCaseVM.DisplayName, testFixtureVM.DisplayName);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        /// <summary>
        /// Open a TestCase to be edited on automation editor
        /// </summary>
        /// <param name="testCaseVM"></param>
        /// <returns></returns>
        public async Task OpenTestCaseAsync(TestCaseViewModel testCaseVM)
        {
            try
            {
               
                if (!testCaseVM.CanOpenForEdit)
                {
                    logger.Information("TestCase {0} is already open for edit", testCaseVM.DisplayName);
                    return;
                }

                ITestCaseFileSystem testCaseFileSystem = this.fileSystem.CreateTestCaseFileSystemFor(testCaseVM.FixtureId);
                string testCaseProcessFile = testCaseFileSystem.GetTestProcessFile(testCaseVM.Id);
                testCaseVM.TestCaseEntity = this.projectManager.Load<Entity>(testCaseProcessFile);
                testCaseVM.TestCaseEntity.Name = testCaseVM.DisplayName;
                testCaseVM.TestCaseEntity.Tag = testCaseVM.Id;

                var parentFixture = this.TestFixtures.First(f => f.Id.Equals(testCaseVM.FixtureId));
                if (!parentFixture.IsOpenForEdit)
                {
                    await this.OpenTestFixtureAsync(parentFixture);
                }

                if (await this.TestRunner.TryOpenTestCase(parentFixture.TestFixture, testCaseVM.TestCase))
                {
                    SetupScriptEditor();
                    testCaseVM.IsOpenForEdit = true;
                    NotifyOfPropertyChange(nameof(CanSaveAll));
                    logger.Information("TestCase {0} is open for edit now", testCaseVM.DisplayName);
                }
                else
                {
                    logger.Warning("Failed to open TestCase {0} for edit.", testCaseVM.DisplayName);
                }
                void SetupScriptEditor()
                {
                    var testEntityManager = testCaseVM.TestCase.TestCaseEntity.EntityManager;
                    IScriptEditorFactory editorFactory = testEntityManager.GetServiceOfType<IScriptEditorFactory>();

                    //Add the test script project and script file to script editor
                    editorFactory.AddProject(testCaseVM.Id, new string[] { parentFixture.Id }, testEntityManager.Arguments.GetType());
                    string scriptFileContent = testCaseFileSystem.ReadAllText(testCaseFileSystem.GetTestScriptFile(testCaseVM.Id));
                    editorFactory.AddDocument(testCaseVM.ScriptFile, testCaseVM.Id, scriptFileContent);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }

        }

        /// <summary>
        ///  Open a TestCase to be edited on automation editor given it's testCaseId
        /// </summary>
        /// <param name="testCaseId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Close a TestCase which was previously opened to be edited on automation editor
        /// </summary>
        /// <param name="testCaseVM"></param>
        /// <param name="autoSave"></param>
        /// <returns></returns>
        public async Task CloseTestCaseAsync(TestCaseViewModel testCaseVM, bool autoSave)
        {
            try
            {
                if (testCaseVM.IsOpenForEdit)
                {                  
                    if (autoSave)
                    {
                        SaveTestCase(testCaseVM);
                    }
                    var parentFixture = this.TestFixtures.First(f => f.Id.Equals(testCaseVM.FixtureId));

                    //Remove the script editor project that was added while opening the test case
                    var entityManager = testCaseVM.TestCaseEntity.EntityManager;
                    var scriptEditorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
                    scriptEditorFactory.RemoveProject(testCaseVM.Id);
                    testCaseVM.TestCaseEntity.DisposeEditors();

                    await this.TestRunner.TryCloseTestCase(parentFixture.TestFixture, testCaseVM.TestCase);

                    testCaseVM.TestCaseEntity = null;
                    testCaseVM.IsOpenForEdit = false;
                    logger.Information("TestCase {0} was closed", testCaseVM.DisplayName);

                }
                NotifyOfPropertyChange(nameof(CanSaveAll));
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        /// <summary>
        /// Close a TestCase given it's testCaseId which was previously opened to be edited on automation editor
        /// </summary>
        /// <param name="testCaseId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Edit the initialization script for the TestCase
        /// </summary>
        /// <param name="testCaseVM"></param>
        /// <returns></returns>
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
                            logger.Information("TestCase script {0} was edited.", testCaseVM.ScriptFile);                          
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

        /// <summary>
        /// Save the details of TestCase locally.
        /// </summary>
        /// <param name="testCaseVM">TestCaseViewModel to be saved</param>
        /// <param name="saveTestEntity">Indicates if the process entity should be saved as well</param>
        public void SaveTestCase(TestCaseViewModel testCaseVM, bool saveTestEntity = true)
        {
            ITestCaseFileSystem testCaseFileSystem = this.fileSystem.CreateTestCaseFileSystemFor(testCaseVM.FixtureId);
            TestFixtureViewModel testFixtureVM = this.TestFixtures.FirstOrDefault(c => c.Id.Equals(testCaseVM.FixtureId));
            if (testFixtureVM != null)
            {
                if (string.IsNullOrEmpty(testCaseVM.ScriptFile))
                {
                    testCaseVM.ScriptFile = testCaseFileSystem.GetRelativePath(testCaseFileSystem.GetTestScriptFile(testCaseVM.Id));
                    testCaseFileSystem.CreateOrReplaceFile(testCaseFileSystem.FixtureDirectory, Path.GetFileName(testCaseVM.ScriptFile), string.Empty);
                }

                //e.g. while assigning test data source , test case is not open for edit and hence will have
                // TestCaseEntity null causing prefab lookup to fail
                if (testCaseVM.IsOpenForEdit)
                {
                    var prefabsUsed = testCaseVM.TestCaseEntity.GetComponentsOfType<PrefabEntity>(SearchScope.Descendants);
                    if (prefabsUsed.Any())
                    {
                        testCaseVM.PrefabReferences = new Core.Models.PrefabReferences();
                        foreach (var prefab in prefabsUsed)
                        {
                            testCaseVM.PrefabReferences.AddPrefabReference(new Core.Models.PrefabReference(prefab.ApplicationId, prefab.PrefabId));
                        }
                    }
                }

                testCaseFileSystem.SaveToFile<TestCase>(testCaseVM.TestCase, testCaseFileSystem.FixtureDirectory);
                if (saveTestEntity)
                {
                    testCaseFileSystem.SaveToFile<Entity>(testCaseVM.TestCaseEntity, testCaseFileSystem.FixtureDirectory, testCaseFileSystem.GetTestProcessFile(testCaseVM.Id));
                }
                logger.Information("Changes to TestCase {0} were saved to local storage.", testCaseVM.DisplayName);
            }
        }

        /// <summary>
        /// Raises a notification to filter the data source by Id in Test data repository view
        /// </summary>
        /// <param name="testCaseVM"></param>
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
        /// Save all open test fixtures and test cases locally.
        /// Changes are not pushed to persistent store by this operation.
        /// </summary>
        public void SaveAll()
        {
            foreach (var fixture in this.TestFixtures)
            {
                try
                {
                    if (fixture.IsOpenForEdit)
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

        bool isSetupComplete;
        /// <summary>
        /// Indicates if the environment setup was done successfully and whether
        /// tests can be executed now.
        /// </summary>
        bool IsSetupComplete
        {
            get => isSetupComplete;
            set
            {
                isSetupComplete = value;
                NotifyOfPropertyChange(() => CanSetUpEnvironment);
                NotifyOfPropertyChange(() => CanTearDownEnvironment);
                NotifyOfPropertyChange(() => CanRunTests);
            }
        }

        /// <summary>
        /// Guard method to check whether tests can be executed
        /// </summary>
        public bool CanRunTests
        {
            get => IsSetupComplete;
        }

        /// <summary>
        /// Guard method to check whether environment setup can be done
        /// </summary>
        public bool CanSetUpEnvironment
        {
            get => !isSetupComplete;
        }

        /// <summary>
        /// Execute the OneTimeSetupEntity for the Automation process
        /// </summary>
        /// <returns></returns>
        public async Task SetUpEnvironmentAsync()
        {
            await this.TestRunner.SetUpEnvironment();
            if(this.TestRunner.CanRunTests)
            {
                this.IsSetupComplete = true;
            }
        }

        /// <summary>
        /// Guard method to check whether environment tear down can be done
        /// </summary>
        public bool CanTearDownEnvironment
        {
            get => isSetupComplete;
        }

        /// <summary>
        /// Execute the OneTimeTearDownEntity for the Automation process
        /// </summary>
        /// <returns></returns>
        public async Task TearDownEnvironmentAsync()
        {
            await this.TestRunner.TearDownEnvironment();
            this.IsSetupComplete = false;
        }

        bool isCancellationRequested = false;

        bool canAbort = false;
        /// <summary>
        /// Guard method to check whether test execution can be aborted
        /// </summary>
        public bool CanAbort
        {
            get => canAbort;
            set
            {
                canAbort = value;
                NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Run selected TestCase
        /// </summary>
        public async Task RunSelected()
        {
            logger.Information("------------------------------------------------------");
            logger.Information("Start execution of selected test case");
            Task runTestCasesTask = new Task(async () =>
            {
                try
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
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message, ex);
                }
            });
            runTestCasesTask.Start();
            await runTestCasesTask;
            logger.Information("------------------------------------------------------");
            logger.Information("Completed execution of selected test case");
        }

        /// <summary>
        /// Run all the available TestCases
        /// </summary>
        public async Task RunAll()
        {
            logger.Information("------------------------------------------------------");
            logger.Information("Start execution of all test cases");
            isCancellationRequested = false;
            CanAbort = true;
            Task runTestCasesTask = new Task(async () =>
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
            await runTestCasesTask;
            logger.Information("------------------------------------------------------");
            logger.Information("Completed execution of all test cases");
        }

        /// <summary>
        /// Abort the execution of TestCase
        /// </summary>
        public void AbortRun()
        {
            isCancellationRequested = true;
        }

        /// <summary>
        /// Try to execute TestCase
        /// </summary>
        /// <param name="testCaseVM"></param>
        /// <returns>Boolean indicating whether the TestCase was executed successfully</returns>
        private async Task<bool> TryRunTestCaseAsync(TestCaseViewModel testCaseVM)
        {
            bool isAlreadyOpenForEdit = testCaseVM.IsOpenForEdit;
            try
            {
                if (testCaseVM.IsMuted || string.IsNullOrEmpty(testCaseVM.TestDataId))
                {
                    return await Task.FromResult(false);
                }

                if (!testCaseVM.IsOpenForEdit)
                {
                    await OpenTestCaseAsync(testCaseVM);
                }
             
                System.Action clearTestResults = () => testCaseVM.TestResults.Clear();   
                platformProvider.OnUIThread(clearTestResults);

                var parentFixture = this.TestFixtures.First(f => f.Id.Equals(testCaseVM.FixtureId));
                await foreach (var result in this.TestRunner.RunTestAsync(parentFixture.TestFixture, testCaseVM.TestCase))
                {
                    System.Action addTestResult = () => testCaseVM.TestResults.Add(result);
                    platformProvider.OnUIThread(addTestResult);
                }

                return await Task.FromResult(true);

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
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

        #region life cycle

        /// <summary>
        /// Called just before view is activiated for the first time.
        /// Fixtures and TestCases are loaded from local storage during initialization.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            Initialize();
            return base.OnInitializeAsync(cancellationToken);
        }

        /// <summary>
        /// Called when the view is deactivated. close will be true when view should be closed as well.
        /// </summary>
        /// <param name="close"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if(close)
            {
                this.TestFixtures.Clear();
                this.TestRunner = null;
            }
            return base.OnDeactivateAsync(close, cancellationToken);

        }

        #endregion life cycle

        #region Notifications        

        /// <summary>
        /// Notification handler for <see cref="TestFilterNotification"/> message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task HandleAsync(TestFilterNotification message, CancellationToken cancellationToken)
        {
            this.FilterText = message?.FilterText ?? string.Empty;
            await Task.CompletedTask;
        }

        #endregion Notifications
    }
}
