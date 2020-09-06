using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Core.Notfications;
using Pixel.Automation.TestExplorer.ViewModels;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Pixel.Automation.TestExplorer
{
    /// <summary>
    /// TestCaseManager manages test cases for active Automation Project 
    /// </summary>
    public class TestRepositoryManager : PropertyChangedBase, IHandle<TestCaseUpdatedEventArgs>
    {
        #region Data members
    
        private readonly ILogger logger = Log.ForContext<TestRepositoryManager>();

        private readonly IProjectFileSystem fileSystem;      
        private readonly IProjectManager projectManager;       
        private readonly IEventAggregator eventAggregator;      
        private readonly IWindowManager windowManager;

        public ITestRunner TestRunner { get; }

        public BindableCollection<TestCategoryViewModel> TestCategories { get; set; } = new BindableCollection<TestCategoryViewModel>();

        public BindableCollection<TestCaseViewModel> OpenTestCases { get; private set; } = new BindableCollection<TestCaseViewModel>();
       
        #endregion Data members

        #region Constructor
  
        public TestRepositoryManager(IEventAggregator eventAggregator, IProjectManager projectManager, IProjectFileSystem fileSystem, ITestRunner testRunner, IWindowManager windowManager)
        {
            this.projectManager = projectManager;
            this.fileSystem = fileSystem;      
            this.TestRunner = testRunner;          
            this.eventAggregator = eventAggregator;            
            this.windowManager = windowManager;
     
            eventAggregator.SubscribeOnPublishedThread(this);

            LoadExistingTests();
        }

        private void LoadExistingTests()
        {           

            foreach (var testCategory in this.fileSystem.LoadFiles<TestCategory>(this.fileSystem.TestCaseRepository))
            {
                TestCategoryViewModel testCategoryVM = new TestCategoryViewModel(testCategory);
                this.TestCategories.Add(testCategoryVM);
            }

            List<TestCaseViewModel> testCases = new List<TestCaseViewModel>();
            foreach (var testDirectory in Directory.GetDirectories(this.fileSystem.TestCaseRepository))
            {
                foreach (var testCase in this.fileSystem.LoadFiles<TestCase>(testDirectory))
                {
                    TestCaseViewModel testCaseVM = new TestCaseViewModel(testCase);
                    testCases.Add(testCaseVM);
                }
            }

            foreach (var testCaseGroup in testCases.GroupBy(t => t.CategoryId))
            {
                var testCategory = this.TestCategories.FirstOrDefault(t => t.Id.Equals(testCaseGroup.Key));
                foreach (var test in testCaseGroup)
                {
                    testCategory.Tests.Add(test);
                }
            }
        }

        #endregion Constructor

        #region Test Category

        public bool CanAddTestCategory
        {
            get => true;
        }

        public async void AddTestCategory()
        {
            TestCategory newTestCategory = new TestCategory()
            {
                DisplayName = $"Category#{TestCategories.Count() + 1}"
            };
            TestCategoryViewModel testCategoryVM = new TestCategoryViewModel(newTestCategory);
            var testCategoryEditor = new EditTestCategoryViewModel(testCategoryVM, this.TestCategories);         
            bool? result = await this.windowManager.ShowDialogAsync(testCategoryEditor);
            if (result.HasValue && result.Value)
            {
                this.TestCategories.Add(testCategoryVM);               
                SaveTestCategory(testCategoryVM);
            }         
        }

        public async void EditTestCategory(TestCategoryViewModel testCategoryVM)
        {
            var testCategoryEditor = new EditTestCategoryViewModel(testCategoryVM , this.TestCategories.Except(new[] { testCategoryVM}));       
            bool? result = await this.windowManager.ShowDialogAsync(testCategoryEditor);
            if (result.HasValue && result.Value)
            {
                SaveTestCategory(testCategoryVM);
            }
        }

        private void SaveTestCategory(TestCategoryViewModel testCategoryVM)
        {
            this.fileSystem.SaveToFile<TestCategory>(testCategoryVM.TestCategory, this.fileSystem.TestCaseRepository);
        }

        public void DeleteTestCategory(TestCategoryViewModel testCategoryVM)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this test category along with all tests?", "Confirm Delete", MessageBoxButton.OKCancel);
            if(result == MessageBoxResult.OK)
            {
                this.TestCategories.Remove(testCategoryVM);
                Directory.Delete(Path.Combine(this.fileSystem.TestCaseRepository, testCategoryVM.Id));
            }
        }       

        #endregion Test Category

        #region Test Case
      
        public void AddTestCase()
        {
            if(this.TestCategories.Any(c => c.IsSelected == true))
            {
                AddTestCase(this.TestCategories.FirstOrDefault(c => c.IsSelected == true));
            }
        }

        public async void AddTestCase(TestCategoryViewModel testCategoryVM)
        {
            TestCase testCase = new TestCase()
            {
                CategoryId = testCategoryVM.Id,
                DisplayName = $"Test#{testCategoryVM.Tests.Count() + 1}",
                TestCaseEntity = new TestCaseEntity() { Name = $"Test#{testCategoryVM.Tests.Count() + 1}" }
            };
            TestCaseViewModel testCaseVM = new TestCaseViewModel(testCase);

            var existingTestCases = TestCategories.SelectMany(c => c.Tests);
            var testCaseEditor = new EditTestCaseViewModel(testCaseVM, existingTestCases);
            IWindowManager windowManager = IoC.Get<IWindowManager>();
            bool? result = await windowManager.ShowDialogAsync(testCaseEditor);
            if (result.HasValue && result.Value)
            {
                testCategoryVM.Tests.Add(testCaseVM);            
                SaveTestCase(testCaseVM);
            }
        }

        public async void EditTestCase(TestCaseViewModel testCaseVM)
        {
            var existingTestCases = TestCategories.SelectMany(c => c.Tests);
            existingTestCases = existingTestCases.Except(new[] { testCaseVM });
            var testCaseEditor = new EditTestCaseViewModel(testCaseVM, existingTestCases);         
            bool? result = await this.windowManager.ShowDialogAsync(testCaseEditor);
            if (result.HasValue && result.Value)
            {
                if(testCaseVM.IsOpenForEdit)
                {
                    testCaseVM.TestCaseEntity.Name = testCaseVM.DisplayName;
                }
                SaveTestCase(testCaseVM, false);
            }
        }

        public void SaveTestCase(TestCaseViewModel testCaseVM, bool saveTestEntity = true)
        {
            ITestCaseFileSystem testCaseFileSystem = this.fileSystem.CreateTestCaseFileSystemFor(testCaseVM.Id);
            TestCategoryViewModel ownerCategory = this.TestCategories.FirstOrDefault(c => c.Id.Equals(testCaseVM.CategoryId));
            if(ownerCategory != null)
            {
                if (!Directory.Exists(testCaseFileSystem.TestDirectory))
                {
                    Directory.CreateDirectory(testCaseFileSystem.TestDirectory);
                }
                if (string.IsNullOrEmpty(testCaseVM.ScriptFile))
                {
                    testCaseVM.ScriptFile = Path.GetRelativePath(testCaseFileSystem.WorkingDirectory, Path.Combine(testCaseFileSystem.ScriptsDirectory, "Variables.csx"));
                    testCaseFileSystem.CreateOrReplaceFile(testCaseFileSystem.ScriptsDirectory, "Variables.csx", string.Empty);
                }

                this.fileSystem.SaveToFile<TestCase>(testCaseVM.TestCase, testCaseFileSystem.TestDirectory, Path.GetFileName(testCaseFileSystem.TestCaseFile));               
                if(saveTestEntity)
                {
                    this.fileSystem.SaveToFile<Entity>(testCaseVM.TestCaseEntity, testCaseFileSystem.TestDirectory, Path.GetFileName(testCaseFileSystem.TestProcessFile));               
                }           
              
            }
        }

        public void DeleteTestCase(TestCaseViewModel testCaseVM)
        {
            if (this.OpenTestCases.Contains(testCaseVM))
            {
                MessageBox.Show("Can't delete test case. Test case is open for edit.", "Confirm Delete", MessageBoxButton.OK);
                return;
            }

            TestCategoryViewModel ownerCategory = this.TestCategories.FirstOrDefault(c => c.Id.Equals(testCaseVM.CategoryId));
            if (ownerCategory != null)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this test", "Confirm Delete", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    string testFile = Path.Combine(this.fileSystem.TestCaseRepository, ownerCategory.Id, testCaseVM.Id);
                    testFile = $"{testFile}.test";
                    File.Delete(testFile);
                    ownerCategory.Tests.Remove(testCaseVM);
                   
                }              
            }
        }      

        public async void OpenForEdit(TestCaseViewModel testCaseVM)
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
                    System.Action openForEditAction = () => OpenForEdit(testCaseVM);
                    dispatcher.Invoke(openForEditAction);
                    return;
                }

                if (this.OpenTestCases.Contains(testCaseVM))
                {
                    return;
                }

                string testCaseProcessFile = Path.Combine(this.fileSystem.TestCaseRepository, testCaseVM.Id, "TestAutomation.proc");
                testCaseVM.TestCaseEntity = this.projectManager.Load<Entity>(testCaseProcessFile);
                testCaseVM.TestCaseEntity.Name = testCaseVM.DisplayName;
                testCaseVM.TestCaseEntity.Tag = testCaseVM.Id;

                if (await this.TestRunner.TryOpenTestCase(testCaseVM.TestCase))
                {
                    SetupScriptEditor();
                    this.OpenTestCases.Add(testCaseVM);
                    testCaseVM.IsOpenForEdit = true;
                    NotifyOfPropertyChange(nameof(CanSaveAll));
                }

                void SetupScriptEditor()
                {
                    var testEntityManager = testCaseVM.TestCase.TestCaseEntity.EntityManager;
                    IScriptEditorFactory editorFactory = testEntityManager.GetServiceOfType<IScriptEditorFactory>();
                    editorFactory.AddProject(testCaseVM.Id, Array.Empty<string>(), testEntityManager.Arguments.GetType());
                    string scriptFileContent = File.ReadAllText(Path.Combine(this.fileSystem.WorkingDirectory, testCaseVM.TestCase.ScriptFile));
                    editorFactory.AddDocument(testCaseVM.TestCase.ScriptFile, testCaseVM.TestCase.Id, scriptFileContent);
                }
                logger.Information($"Test Case : {testCaseVM.DisplayName} is open for edit now.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }

        }

        public void OpenForEdit(string testCaseId)
        {
            foreach (var testCategory in this.TestCategories)
            {
                var targetTestCase = testCategory.Tests.FirstOrDefault(t => t.Id.Equals(testCaseId));
                if (targetTestCase != null)
                {
                    OpenForEdit(targetTestCase);
                    break;
                }
            }
        }

        public void DoneEditing(TestCaseViewModel testCaseVM, bool autoSave)
        {
            if (this.OpenTestCases.Contains(testCaseVM))
            {
                logger.Information($"Trying to close test case : {testCaseVM.DisplayName} for edit.");

                if (autoSave)
                {
                    SaveTestCase(testCaseVM);
                }
                testCaseVM.TestResults.Clear();
                this.TestRunner.CloseTestCase(testCaseVM.TestCase);
                this.OpenTestCases.Remove(testCaseVM);

                testCaseVM.TestCaseEntity = null;
                testCaseVM.IsOpenForEdit = false;
                logger.Information($"Test Case {testCaseVM.DisplayName} has been closed.");

            }
            NotifyOfPropertyChange(nameof(CanSaveAll));
        }

        public void DoneEditing(string testCaseId)
        {           
            var targetTestCase = OpenTestCases.FirstOrDefault(a => a.Id.Equals(testCaseId));
            DoneEditing(targetTestCase, true);
           
        }

        public bool CanSaveAll
        {
            get => this.OpenTestCases.Count > 0;
        }

        public void SaveAllOpenTests()
        {
            foreach(var test in this.OpenTestCases)
            {
                try
                {
                    SaveTestCase(test);
                }
                catch (Exception ex)
                {
                    logger.Error(ex,ex.Message);
                }
            }
        }         
       
        public async void EditScript(TestCaseViewModel testCaseVM)
        {
            if(testCaseVM.IsOpenForEdit)
            {
                var entityManager = testCaseVM.TestCaseEntity.EntityManager;
                var scriptEditorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
                IScriptEditorScreen scriptEditorScreen = scriptEditorFactory.CreateScriptEditor();
                scriptEditorScreen.OpenDocument(testCaseVM.ScriptFile, testCaseVM.Id, string.Empty);
                var result = await this.windowManager.ShowDialogAsync(scriptEditorScreen);
                if (result.HasValue && result.Value)
                {
                    var scriptEngine = entityManager.GetScriptEngine();
                    scriptEngine.ClearState();
                    await scriptEngine.ExecuteFileAsync(testCaseVM.ScriptFile);
                }
            }
         
        }

        public async void ShowDataSource(TestCaseViewModel testCaseVM)
        {
            await this.eventAggregator.PublishOnUIThreadAsync(new ShowTestDataSourceNotification(testCaseVM.TestDataId));
        }


        #endregion Test Case

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
                foreach (var testCategory in this.TestCategories)
                {
                    foreach (var test in testCategory.Tests)
                    {
                        if (test.IsSelected)
                        {
                           await TryRunTestCaseAsync(test);
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
                foreach (var testCategory in this.TestCategories)
                {
                    foreach (var test in testCategory.Tests)
                    {
                        if (!isCancellationRequested)
                        {
                          await TryRunTestCaseAsync(test);
                        }                           
                    }
                }
                CanAbort = false;
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
                    OpenForEdit(testCaseVM);
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

                await foreach (var result in this.TestRunner.RunTestAsync(testCaseVM.TestCase))
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
                    DoneEditing(testCaseVM, false);
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
                var targetTestCase = this.TestCategories.SelectMany(c => c.Tests).FirstOrDefault(t => t.Id.Equals(message.ModifiedTestCase.Id));               
                this.SaveTestCase(targetTestCase, false);
            }
            await Task.CompletedTask;
        }

        #endregion Notifications
    }

}

