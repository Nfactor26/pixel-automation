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
using System.Diagnostics;
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
            foreach(var testCategory in this.fileSystem.LoadFiles<TestCategory>(this.fileSystem.TestCaseDirectory))
            {
                TestCategoryViewModel testCategoryVM = new TestCategoryViewModel(testCategory);
                this.TestCategories.Add(testCategoryVM);
             
                var testDirectory = Path.Combine(this.fileSystem.TestCaseDirectory, testCategory.Id);
                if(!Directory.Exists(testDirectory))
                {
                    //No test case has been added to test category yet.
                    continue;
                }

                foreach (var testCase in this.fileSystem.LoadFiles<TestCase>(testDirectory))
                {                  
                    TestCaseViewModel testCaseVM = new TestCaseViewModel(testCase);
                    testCategoryVM.Tests.Add(testCaseVM);
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
            this.fileSystem.SaveToFile<TestCategory>(testCategoryVM.TestCategory, this.fileSystem.TestCaseDirectory);
        }

        public void DeleteTestCategory(TestCategoryViewModel testCategoryVM)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this test category along with all tests?", "Confirm Delete", MessageBoxButton.OKCancel);
            if(result == MessageBoxResult.OK)
            {
                this.TestCategories.Remove(testCategoryVM);
                Directory.Delete(Path.Combine(this.fileSystem.TestCaseDirectory, testCategoryVM.Id));
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
                TestCaseEntity = new TestCaseEntity() { Name = $"Test#{testCategoryVM.Tests.Count() + 1}" },
                ScriptFile = $"{Guid.NewGuid().ToString()}.csx"
            };
            TestCaseViewModel testCaseVM = new TestCaseViewModel(testCase);

            var existingTestCases = TestCategories.SelectMany(c => c.Tests);
            var testCaseEditor = new EditTestCaseViewModel(testCaseVM, existingTestCases);
            IWindowManager windowManager = IoC.Get<IWindowManager>();
            bool? result = await windowManager.ShowDialogAsync(testCaseEditor);
            if (result.HasValue && result.Value)
            {
                testCategoryVM.Tests.Add(testCaseVM);
                this.fileSystem.CreateOrReplaceFile(this.fileSystem.ScriptsDirectory, testCaseVM.ScriptFile, string.Empty);
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
            TestCategoryViewModel ownerCategory = this.TestCategories.FirstOrDefault(c => c.Id.Equals(testCaseVM.CategoryId));
            if(ownerCategory != null)
            {
                this.fileSystem.SaveToFile<TestCase>(testCaseVM.TestCase, Path.Combine(this.fileSystem.TestCaseDirectory, ownerCategory.Id));               
                if(saveTestEntity)
                {
                    this.fileSystem.SaveToFile<Entity>(testCaseVM.TestCaseEntity, Path.Combine(this.fileSystem.TestCaseDirectory, ownerCategory.Id), $"{testCaseVM.Id}.atm");               
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
                    string testFile = Path.Combine(this.fileSystem.TestCaseDirectory, ownerCategory.Id, testCaseVM.Id);
                    testFile = $"{testFile}.test";
                    File.Delete(testFile);
                    ownerCategory.Tests.Remove(testCaseVM);
                   
                }              
            }
        }      

        public async void OpenForEdit(TestCaseViewModel testCaseVM)
        {
            if (!testCaseVM.CanOpenForEdit)
            {
                return;
            }

            //This is required because OpenForEdit might be called from RunAll for tests which are not open for edit.
            //Opening a test initializes dependencies like script engine , script editor , etc for test case.
            //script editor can only be created on dispatcher thread although it won't be used while running.
            //A better design would be if we could somehow distinguish between design time and run time and configure required services accordingly.
            Dispatcher dispatcher = System.Windows.Application.Current.Dispatcher;
            if(!dispatcher.CheckAccess())
            {
                System.Action openForEditAction = () => OpenForEdit(testCaseVM);
                dispatcher.Invoke(openForEditAction);
                return;
            }

            if (this.OpenTestCases.Contains(testCaseVM))
                return;

            TestCategoryViewModel ownerCategory = this.TestCategories.FirstOrDefault(c => c.Id.Equals(testCaseVM.CategoryId));        
            string testCaseProcessFile = Path.Combine(this.fileSystem.TestCaseDirectory, ownerCategory.Id, $"{testCaseVM.Id}.atm");
            testCaseVM.TestCaseEntity = this.projectManager.Load<Entity>(testCaseProcessFile);
            testCaseVM.TestCaseEntity.Tag = testCaseVM.Id;

            await this.TestRunner.OpenTestCase(testCaseVM.TestCase);
           
            this.OpenTestCases.Add(testCaseVM);
            testCaseVM.IsOpenForEdit = true;
            NotifyOfPropertyChange(nameof(CanSaveAll));

        }

        public void OpenForEdit(string testCaseId)
        {
            foreach (var testCategory in this.TestCategories)
            {
                var targetTestCase = testCategory.Tests.FirstOrDefault(t => t.Id.Equals(testCaseId));
                if (targetTestCase != null)
                {
                    OpenForEdit(targetTestCase);
                    logger.Information($"Test Case : {targetTestCase} is open for edit now.");
                    break;
                }
            }
        }

        public void DoneEditing(TestCaseViewModel testCaseVM, bool autoSave)
        {
            if (this.OpenTestCases.Contains(testCaseVM))
            {
                if (autoSave)
                {
                    SaveTestCase(testCaseVM);
                }
                
                this.TestRunner.CloseTestCase(testCaseVM.TestCase);
                this.OpenTestCases.Remove(testCaseVM);

                testCaseVM.TestCaseEntity = null;
                testCaseVM.IsOpenForEdit = false;

            }         
            NotifyOfPropertyChange(nameof(CanSaveAll));
        }

        public void DoneEditing(string testCaseId)
        {
            var targetTestCase = OpenTestCases.FirstOrDefault(a => a.Id.Equals(testCaseId));
            if (targetTestCase != null)
            {
                SaveTestCase(targetTestCase);
                targetTestCase.TestCaseEntity = null;
                targetTestCase.TestResults.Clear();
                this.OpenTestCases.Remove(targetTestCase);
                targetTestCase.IsOpenForEdit = false;
                NotifyOfPropertyChange(nameof(CanSaveAll));
                return;
            }
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
                scriptEditorScreen.OpenDocument(testCaseVM.ScriptFile, string.Empty);
                var result = await this.windowManager.ShowDialogAsync(scriptEditorScreen);
                if (result.HasValue && result.Value)
                {
                    var scriptEngine = entityManager.GetServiceOfType<IScriptEngine>();
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

