using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Core.Notfications;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Collections;
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
    public class TestCaseManager : PropertyChangedBase, IHandle<TestCaseUpdatedEventArgs>
    {
        #region Data members
    
        private readonly IProjectFileSystem fileSystem;
        private readonly ISerializer serializer;     
        private readonly IProjectManager projectManager;
        private readonly ITestDataLoader testDataManager;
        private readonly IEventAggregator eventAggregator;      
        private readonly IWindowManager windowManager;

        public ITestRunner TestRunner { get; }

        public BindableCollection<TestCategory> TestCategories { get; set; } = new BindableCollection<TestCategory>();

        public BindableCollection<TestCase> OpenTestCases { get; private set; } = new BindableCollection<TestCase>();
       
        #endregion Data members

        #region Constructor
        public TestCaseManager(IEventAggregator eventAggregator, IProjectManager projectManager, IProjectFileSystem fileSystem, ISerializer serializer, ITestRunner testRunner, ITestDataLoader testDataManager, IWindowManager windowManager)
        {
            this.projectManager = projectManager;
            this.fileSystem = fileSystem;
            this.serializer = serializer;        
            this.TestRunner = testRunner;
            this.testDataManager = testDataManager;
            this.eventAggregator = eventAggregator;            
            this.windowManager = windowManager;
     
            eventAggregator.SubscribeOnPublishedThread(this);

            LoadExistingTests();
        }

        private void LoadExistingTests()
        {
            foreach (var catFile in Directory.GetFiles(this.fileSystem.TestCaseDirectory))
            {
                //TODO : Validate file has proper type
                TestCategory testCategory = this.serializer.Deserialize<TestCategory>(catFile);              
                this.TestCategories.Add(testCategory);
                foreach(var testFile in Directory.GetFiles(Path.Combine(this.fileSystem.TestCaseDirectory, testCategory.Id),"*.test"))
                {
                    TestCase testCase = this.serializer.Deserialize<TestCase>(testFile);
                    testCategory.Tests.Add(testCase);
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
            TestCategory testCategory = new TestCategory()
            {
                DisplayName = $"Category#{TestCategories.Count() + 1}"
            };
            var testCategoryEditor = new TestCategoryViewModel(testCategory, this.TestCategories);
            IWindowManager windowManager = IoC.Get<IWindowManager>();
            bool? result = await windowManager.ShowDialogAsync(testCategoryEditor);
            if (result.HasValue && result.Value)
            {
                this.TestCategories.Add(testCategory);               
                SaveTestCategory(testCategoryEditor.CopyOfTestCategory);
            }         
        }

        public async void EditTestCategory(TestCategory testCategory)
        {
            var testCategoryEditor = new TestCategoryViewModel(testCategory , this.TestCategories.Except(new[] { testCategory}));
            IWindowManager windowManager = IoC.Get<IWindowManager>();
            bool? result = await windowManager.ShowDialogAsync(testCategoryEditor);
            if (result.HasValue && result.Value)
            {
                SaveTestCategory(testCategory);
            }
        }

        private void SaveTestCategory(TestCategory testCategory)
        {         
            if(!Directory.Exists(Path.Combine(this.fileSystem.TestCaseDirectory,testCategory.Id)))
            {
                Directory.CreateDirectory(Path.Combine(this.fileSystem.TestCaseDirectory, testCategory.Id));
            }
            this.serializer.Serialize<TestCategory>(Path.Combine(this.fileSystem.TestCaseDirectory, $"{testCategory.Id}.tcat"),
                testCategory);          
        }

        public void DeleteTestCategory(TestCategory testCategory)
        {
            MessageBoxResult result =MessageBox.Show("Are you sure you want to delete this test category along with all tests?", "Confirm Delete", MessageBoxButton.OKCancel);
            if(result == MessageBoxResult.OK)
            {
                this.TestCategories.Remove(testCategory);
                Directory.Delete(Path.Combine(this.fileSystem.TestCaseDirectory, testCategory.Id));
            }
        }       

        #endregion Test Category

        #region Test Case
      
        public void AddTestCase()
        {
            if(this.TestCategories.Any(c => c.IsSelected == true))
                AddTestCase(this.TestCategories.FirstOrDefault(c => c.IsSelected == true));
        }

        public async void AddTestCase(TestCategory testCategory)
        {
            TestCase testCase = new TestCase()
            {
                CategoryId = testCategory.Id,
                DisplayName = $"Test#{testCategory.Tests.Count() + 1}",
                TestCaseEntity = new TestCaseEntity() { Name = $"Test#{testCategory.Tests.Count() + 1}" },
                ScriptFile = $"{Guid.NewGuid().ToString()}.csx"
            };

            var existingTestCases = TestCategories.SelectMany(c => c.Tests);
            var testCaseEditor = new TestCaseViewModel(testCase, existingTestCases);
            IWindowManager windowManager = IoC.Get<IWindowManager>();
            bool? result = await windowManager.ShowDialogAsync(testCaseEditor);
            if (result.HasValue && result.Value)
            {
                testCategory.Tests.Add(testCase);
                SaveTestCase(testCase);
            }
        }

        public async void EditTestCase(TestCase testCase)
        {
            var existingTestCases = TestCategories.SelectMany(c => c.Tests);
            existingTestCases = existingTestCases.Except(new[] { testCase });
            var testCaseEditor = new TestCaseViewModel(testCase, existingTestCases);
            IWindowManager windowManager = IoC.Get<IWindowManager>();
            bool? result = await windowManager.ShowDialogAsync(testCaseEditor);
            if (result.HasValue && result.Value)
            {
                if(testCase.IsOpenForEdit)
                {
                    testCase.TestCaseEntity.Name = testCase.DisplayName;
                }
                SaveTestCase(testCase, false);
            }
        }

        public void SaveTestCase(TestCase testCase, bool saveTestEntity = true)
        {
            TestCategory ownerCategory = this.TestCategories.FirstOrDefault(c => c.Id.Equals(testCase.CategoryId));
            if(ownerCategory != null)
            {
                string testCaseFile = Path.Combine(this.fileSystem.TestCaseDirectory, ownerCategory.Id, $"{testCase.Id}.test");            
                this.serializer.Serialize<TestCase>(testCaseFile, testCase);

                string scriptFile = Path.Combine(this.fileSystem.ScriptsDirectory, testCase.ScriptFile);
                if(!File.Exists(scriptFile))
                {
                    File.Create(scriptFile);
                }

                if(saveTestEntity)
                {
                    string testCaseProcessFile = Path.Combine(this.fileSystem.TestCaseDirectory, ownerCategory.Id, $"{testCase.Id}.atm");
                    this.serializer.Serialize<Entity>(testCaseProcessFile, testCase.TestCaseEntity);
                }               
            }
        }

        public void DeleteTestCase(TestCase testCase)
        {
            if (this.OpenTestCases.Contains(testCase))
            {
                MessageBox.Show("Can't delete test case. Test case is open for edit.", "Confirm Delete", MessageBoxButton.OK);
                return;
            }

            TestCategory ownerCategory = this.TestCategories.FirstOrDefault(c => c.Id.Equals(testCase.CategoryId));
            if (ownerCategory != null)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this test", "Confirm Delete", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    string testFile = Path.Combine(this.fileSystem.TestCaseDirectory, ownerCategory.Id, testCase.Id);
                    testFile = $"{testFile}.test";
                    File.Delete(testFile);
                    ownerCategory.Tests.Remove(testCase);
                   
                }              
            }
        }

        public async void OpenForEdit(TestCase testCase)
        {
            //This is required because OpenForEdit might be called from RunAll for tests which are not open for edit.
            //Opening a test initializes dependencies like script engine , script editor , etc for test case.
            //script editor can only be created on dispatcher thread although it won't be used while running.
            //A better design would be if we could somehow distinguish between design time and run time and configure required services accordingly.
            Dispatcher dispatcher = System.Windows.Application.Current.Dispatcher;
            if(!dispatcher.CheckAccess())
            {
                System.Action openForEditAction = () => OpenForEdit(testCase);
                dispatcher.Invoke(openForEditAction);
                return;
            }

            if (this.OpenTestCases.Contains(testCase))
                return;

            TestCategory ownerCategory = this.TestCategories.FirstOrDefault(c => c.Id.Equals(testCase.CategoryId));        
            string testCaseProcessFile = Path.Combine(this.fileSystem.TestCaseDirectory, ownerCategory.Id, $"{testCase.Id}.atm");
            testCase.TestCaseEntity = this.projectManager.Load<Entity>(testCaseProcessFile);
            testCase.TestCaseEntity.Tag = testCase.Id;

            this.TestRunner.OpenTestEntity(testCase);
            EntityManager entityManager = testCase.TestCaseEntity.EntityManager;           

            var dataSource = testDataManager.GetTestCaseData(testCase);
            if (dataSource is IEnumerable dataSourceCollection)
            {
                testCase.TestCaseEntity.EntityManager.Arguments = dataSource.FirstOrDefault();
            }

            //Add script file initially to workspace so that other scripts can have intellisense support for any script variables defined in this file
            IScriptEditorFactory scriptEditor = entityManager.GetServiceOfType<IScriptEditorFactory>();
            var workspaceManager = scriptEditor.GetWorkspaceManager();
            string scriptFileContent = File.ReadAllText(Path.Combine(fileSystem.ScriptsDirectory, testCase.ScriptFile));
            workspaceManager.AddDocument(testCase.ScriptFile, scriptFileContent);

            //Execute script file to set up initial state of script engine
            IScriptEngine scriptEngine = entityManager.GetServiceOfType<IScriptEngine>();          
            //scriptEngine.SetGlobals(testCase.TestCaseEntity.EntityManager.Arguments);
            await scriptEngine.ExecuteScriptAsync(scriptFileContent);


            this.OpenTestCases.Add(testCase);
            testCase.IsOpenForEdit = true;
            NotifyOfPropertyChange(nameof(CanSaveAll));

        }

        public void DoneEditing(TestCase testCase, bool autoSave)
        {
            if (this.OpenTestCases.Contains(testCase))
            {
                if (autoSave)
                {
                    SaveTestCase(testCase);
                }
                
                this.TestRunner.RemoveTestEntity(testCase.TestCaseEntity);
                this.OpenTestCases.Remove(testCase);

                testCase.TestCaseEntity = null;
                testCase.IsOpenForEdit = false;

            }         
            NotifyOfPropertyChange(nameof(CanSaveAll));
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
                    Debug.WriteLine(ex.Message);
                    //TODO : Log error
                }
            }
        }      

        public void DoneEditing(string testCaseId)
        {
            var targetTestCase = OpenTestCases.FirstOrDefault(a => a.Id.Equals(testCaseId));
            if(targetTestCase != null)
            {
                SaveTestCase(targetTestCase);
                targetTestCase.TestCaseEntity = null;
                targetTestCase.TestResults.Clear();
                this.OpenTestCases.Remove(targetTestCase);
                targetTestCase.IsOpenForEdit = false;
                NotifyOfPropertyChange(nameof(CanSaveAll));
            }
        }

        public void OpenForEdit(string testCaseId)
        {
            foreach(var testCategory in this.TestCategories)
            {
                var targetTestCase = testCategory.Tests.FirstOrDefault(t => t.Id.Equals(testCaseId));
                if(targetTestCase != null)
                {
                    OpenForEdit(targetTestCase);
                    break;
                }
            }
        }

        public async void EditScript(TestCase testCase)
        {
            var scriptEditorFactory = testCase.TestCaseEntity.EntityManager.GetServiceOfType<IScriptEditorFactory>();
            IScriptEditorScreen scriptEditorScreen = scriptEditorFactory.CreateScriptEditor();
            scriptEditorScreen.OpenDocument(testCase.ScriptFile, string.Empty);
            var result = await this.windowManager.ShowDialogAsync(scriptEditorScreen);          
        }

        public async void ShowDataSource(TestCase testCase)
        {
            await this.eventAggregator.PublishOnUIThreadAsync(new ShowTestDataSourceNotification(testCase.TestDataId));
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

        private async Task<bool> TryRunTestCaseAsync(TestCase testCase)
        {           
            bool isAlreadyOpenForEdit = testCase.IsOpenForEdit;
            try
            {
                if (testCase.IsMuted)
                {
                   return await Task.FromResult(false);
                }

                if (!testCase.IsOpenForEdit)
                {
                    OpenForEdit(testCase);
                }
                else
                {
                    TestCaseEntity testCaseEntity = testCase.TestCaseEntity as TestCaseEntity;
                    var dataSource = testDataManager.GetTestCaseData(testCase);


                    if (dataSource is IEnumerable dataSourceCollection)
                    {
                        Dispatcher dispatcher = System.Windows.Application.Current.Dispatcher;
                        System.Action clearTestResults = () => testCase.TestResults.Clear();
                        if (!dispatcher.CheckAccess())
                        {
                            dispatcher.Invoke(clearTestResults);
                        }
                        else
                        {
                            clearTestResults();
                        }

                        foreach (var item in dataSourceCollection)
                        {
                            testCaseEntity.EntityManager.Arguments = item;
                            IScriptEngine scriptEngine = testCaseEntity.EntityManager.GetServiceOfType<IScriptEngine>();
                            //scriptEngine.SetGlobals(item.ToScriptArguments(testCaseEntity.EntityManager));
                            var state = await scriptEngine.ExecuteFileAsync(testCase.ScriptFile);
                            testCase.IsRunning = true;
                            TestResult result = await this.TestRunner.RunTestAsync(testCase.TestCaseEntity);

                            System.Action addTestResult = () => testCase.TestResults.Add(result);
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
                }             

                throw new ConfigurationException($"Failed to run TestCase : {testCase.DisplayName}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return await Task.FromResult(false);
            }
            finally
            {
                if(!isAlreadyOpenForEdit)
                {
                    DoneEditing(testCase, false);
                }
                testCase.IsRunning = false;
            }
        }

        #endregion Execute       


        /// <summary>
        /// This will be called whenever a new test data source is dragged on a test case.
        /// When this happens, TestDataModel property of test case is updated and we need to save back the details
        /// </summary>
        /// <param name="message"></param>
        public async Task HandleAsync(TestCaseUpdatedEventArgs message, CancellationToken cancellationToken)
        {
            if (message != null && message.ModifiedTestCase != null)
            {
                this.SaveTestCase(message.ModifiedTestCase, false);
            }
            await Task.CompletedTask;
        }
    }

}

