using Caliburn.Micro;
using Dawn;
using GongSolutions.Wpf.DragDrop;
using Notifications.Wpf.Core;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Notifications;
using Pixel.Automation.TestExplorer.Views;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    /// <summary>
    /// TestExplorerViewModel holds the TestFixtures and TestCases belonging to an Automation Project
    /// and allows various operations such as adding, editing, deleting, etc. Any modifications are only locall stored.
    /// Changes are pushed to a persistent store only when project is saved. 
    /// It also allows to execute test cases and see results at design time.
    /// </summary>
    public class TestExplorerViewModel : Screen, ITestExplorer, IHandle<ControlAddedEventArgs>, IHandle<ControlRemovedEventArgs>,
        IHandle<PrefabAddedEventArgs>,  IHandle<PrefabRemovedEventArgs>, 
        IHandle<TestFilterNotification>
    {
        #region Data members

        private readonly ILogger logger = Log.ForContext<TestExplorerViewModel>();

        private readonly IProjectFileSystem fileSystem;
        private readonly IProjectManager projectManager;
        private readonly IComponentViewBuilder componentViewBuilder;
        private readonly IEventAggregator eventAggregator;
        private readonly IWindowManager windowManager;
        private readonly INotificationManager notificationManager;
        private readonly IPlatformProvider platformProvider;
        private readonly ITestCaseManager testCaseManager;
        private readonly ITestFixtureManager testFixtureManager;
        private readonly ApplicationSettings applicationSettings;
        private bool isInitialized = false;

        /// <summary>
        /// TestRunner allows execution of test cases and setup and tear down of environment
        /// </summary>
        public ITestRunner TestRunner { get; private set; }

        /// <summary>
        /// Handler to support drag drop of <see cref="TestDataSource"/> to test cases
        /// </summary>
        public IDropTarget TestDataSourceDropHandler { get;}

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
        public TestExplorerViewModel(IEventAggregator eventAggregator, IAutomationProjectManager projectManager, 
            IProjectFileSystem fileSystem, ITestRunner testRunner, IProjectAssetsDataManager projectAssetsDataManager,
            IComponentViewBuilder componentViewBuilder, IWindowManager windowManager, INotificationManager notificationManager,
            IPlatformProvider platformProvider, ApplicationSettings applicationSettings)
        {
            this.projectManager = Guard.Argument(projectManager, nameof(projectManager)).NotNull().Value;
            this.componentViewBuilder = Guard.Argument(componentViewBuilder, nameof(componentViewBuilder)).NotNull().Value;
            this.fileSystem = Guard.Argument(fileSystem, nameof(fileSystem)).NotNull().Value;
            this.TestRunner = Guard.Argument(testRunner, nameof(testRunner)).NotNull().Value;
            this.testCaseManager = Guard.Argument(projectAssetsDataManager, nameof(projectAssetsDataManager)).NotNull().Value;
            this.testFixtureManager = Guard.Argument(projectAssetsDataManager, nameof(projectAssetsDataManager)).NotNull().Value;
            this.eventAggregator = Guard.Argument(eventAggregator, nameof(eventAggregator)).NotNull().Value;
            this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;
            this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;
            this.platformProvider = Guard.Argument(platformProvider, nameof(platformProvider)).NotNull().Value;
            this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
            this.eventAggregator.SubscribeOnPublishedThread(this);
            this.TestDataSourceDropHandler = new TestDataSourceDropHandler(this.testCaseManager);

            CreateDefaultView();
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
                    foreach (var test in fixture.Tests)
                    {
                        test.UpdateVisibility(filterText);
                    }
                    return fixture.IsVisible;
                }
                return true;
            });
        }

        /// <summary>
        /// Download and load the fixtures and test cases
        /// </summary>
        async Task LoadFixturesAsync()
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(LoadFixturesAsync), ActivityKind.Internal))
            {
                try
                {
                    if (!isInitialized)
                    {
                        await this.testFixtureManager.DownloadAllFixturesAsync();
                        await this.testCaseManager.DownloadAllTestsAsync();
                        foreach (var testFixtureDirectory in Directory.GetDirectories(this.fileSystem.TestCaseRepository))
                        {
                            var testFixture = this.fileSystem.LoadFiles<TestFixture>(testFixtureDirectory).Single();
                            if (testFixture.IsDeleted)
                            {
                                continue;
                            }
                            TestFixtureViewModel testFixtureVM = new TestFixtureViewModel(testFixture);
                            this.TestFixtures.Add(testFixtureVM);
                        }
                        isInitialized = true;
                        logger.Information("Loaded {0} test fixtures from local storage for test explorer", this.TestFixtures.Count());
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to load the fixtures");
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }             
        }

        /// <summary>
        /// Load all test cases for a given fixture
        /// </summary>
        /// <param name="testFixtureViewModel"></param>
        /// <returns></returns>
        async Task LoadTestCasesForFixture(TestFixtureViewModel testFixtureViewModel)
        {
            try
            {
                Guard.Argument(testFixtureViewModel, nameof(testFixtureViewModel)).NotNull();
                foreach (var testCaseDirectory in Directory.GetDirectories(Path.Combine(this.fileSystem.TestCaseRepository, testFixtureViewModel.FixtureId)))
                {
                    var testCase = this.fileSystem.LoadFiles<TestCase>(testCaseDirectory).Single();
                    if (testCase.IsDeleted)
                    {
                        continue;
                    }
                    TestCaseViewModel testCaseVM = new TestCaseViewModel(testCase);
                    testFixtureViewModel.Tests.Add(testCaseVM);
                }
                logger.Information("Loaded {0} test cases for fixture : '{1}'", testFixtureViewModel.Tests.Count(), testFixtureViewModel.DisplayName);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to load test cases for fixture : {0}", testFixtureViewModel?.DisplayName);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }        

        /// <summary>
        /// When a fixture is expanded on view, load all the test cases belonging to it
        /// </summary>
        /// <param name="testFixtureViewModel"></param>
        /// <returns></returns>
        public async Task OnFixtureExpanded(TestFixtureViewModel testFixtureViewModel)
        {
            if(!testFixtureViewModel.Tests.Any())
            {
                await LoadTestCasesForFixture(testFixtureViewModel);
            }
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
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(AddTestFixtureAsync), ActivityKind.Internal))
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
                        PostDelay = applicationSettings.PostDelay,
                        DelayFactor = applicationSettings.DelayFactor
                    };
                    var testFixtureEditor = new EditTestFixtureViewModel(newTestFixture, this.TestFixtures.Select(s => s.DisplayName));
                    bool? result = await this.windowManager.ShowDialogAsync(testFixtureEditor);
                    if (result.HasValue && result.Value)
                    {
                        await this.testFixtureManager.AddTestFixtureAsync(newTestFixture);
                        this.TestFixtures.Add(testFixtureVM);
                        logger.Information("Added new fixture : '{0}'", newTestFixture.DisplayName);
                        activity?.SetTag("Fixture", newTestFixture.DisplayName);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error while trying to add new test fixture");
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }          
        }

        /// <summary>
        /// Edit the details of an existing TestFixture
        /// </summary>
        /// <param name="fixtureVM"></param>
        /// <returns></returns>
        public async Task EditTestFixtureAsync(TestFixtureViewModel fixtureVM)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(EditTestFixtureAsync), ActivityKind.Internal))
            {
                try
                {
                    Guard.Argument(fixtureVM, nameof(fixtureVM)).NotNull();
                    activity?.SetTag("Fixture", fixtureVM.DisplayName);
                    var existingFixtures = this.TestFixtures.Except(new[] { fixtureVM }).Select(f => f.DisplayName);
                    var testFixtureEditor = new EditTestFixtureViewModel(fixtureVM.TestFixture, existingFixtures);
                    bool? result = await this.windowManager.ShowDialogAsync(testFixtureEditor);
                    if (result.HasValue && result.Value)
                    {
                        fixtureVM.Refresh();
                        await this.testFixtureManager.UpdateTestFixtureAsync(fixtureVM.TestFixture);
                        fixtureVM.IsDirty = false;
                        logger.Information("Fixture : '{0}' was edited.", fixtureVM.DisplayName);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error while trying to edit test fixture : '{0}'", fixtureVM?.DisplayName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }              
        }

        /// <summary>
        /// Delete an existing TestFixture
        /// </summary>
        /// <param name="fixtureVM"></param>
        public async Task DeleteTestFixtureAsync(TestFixtureViewModel fixtureVM)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this test fixture along with all tests?", "Confirm Delete", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(DeleteTestFixtureAsync), ActivityKind.Internal))
                {
                    try
                    {
                        Guard.Argument(fixtureVM, nameof(fixtureVM)).NotNull();
                        activity?.SetTag("Fixture", fixtureVM.DisplayName);
                        await this.testFixtureManager.DeleteTestFixtureAsync(fixtureVM.TestFixture);
                        this.TestFixtures.Remove(fixtureVM);
                        logger.Information("Test fixture : '{0}' was deleted.", fixtureVM.DisplayName);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Error while trying to delete test fixture : '{0}'", fixtureVM?.DisplayName);
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await notificationManager.ShowErrorNotificationAsync(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Open a TestFixture for editing on automation editor
        /// </summary>
        /// <param name="fixtureVM"></param>
        /// <returns></returns>
        public async Task OpenTestFixtureAsync(TestFixtureViewModel fixtureVM)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(OpenTestFixtureAsync), ActivityKind.Internal))
            {
                try
                {
                    Guard.Argument(fixtureVM, nameof(fixtureVM)).NotNull();
                    activity?.SetTag("Fixture", fixtureVM.DisplayName);

                    if (!fixtureVM.CanOpenForEdit)
                    {
                        logger.Information("Test fixture : '{0}' is already open for edit.", fixtureVM.DisplayName);
                        return;
                    }

                    await this.testFixtureManager.DownloadFixtureDataAsync(fixtureVM.TestFixture);

                    var fixtureFiles = this.fileSystem.GetTestFixtureFiles(fixtureVM.TestFixture);
                    fixtureVM.TestFixtureEntity = this.projectManager.Load<Entity>(fixtureFiles.ProcessFile);
                    fixtureVM.TestFixtureEntity.Name = fixtureVM.DisplayName;
                    fixtureVM.TestFixtureEntity.Tag = fixtureVM.FixtureId;

                    if (await this.TestRunner.TryOpenTestFixture(fixtureVM.TestFixture))
                    {
                        OpenInEditor();
                        SetupScriptEditor();
                        fixtureVM.IsOpenForEdit = true;
                        NotifyOfPropertyChange(nameof(CanSaveAll));
                    }

                    void OpenInEditor()
                    {
                        if (!fixtureVM.OpenForExecute)
                        {
                            this.componentViewBuilder.OpenTestFixture(fixtureVM.TestFixture);
                        }
                    }

                    void SetupScriptEditor()
                    {
                        if (!fixtureVM.OpenForExecute)
                        {
                            var fixtureEntityManager = fixtureVM.TestFixtureEntity.EntityManager;
                            IScriptEditorFactory editorFactory = fixtureEntityManager.GetServiceOfType<IScriptEditorFactory>();
                            editorFactory.AddProject(fixtureVM.FixtureId, Array.Empty<string>(), typeof(EmptyModel));
                            string fixtureScriptContent = this.fileSystem.ReadAllText(fixtureFiles.ScriptFile);
                            editorFactory.AddDocument(fixtureVM.ScriptFile, fixtureVM.FixtureId, fixtureScriptContent);
                        }

                    }
                    logger.Information("Test fixture : '{0}' is open for edit now.", fixtureVM.DisplayName);
                }
                catch (Exception ex)
                {
                    fixtureVM.OpenForExecute = false;
                    logger.Error(ex, "Error while trying to open test fixture : '{0}'", fixtureVM?.DisplayName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
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
                Guard.Argument(fixtureId, nameof(fixtureId)).NotNull().NotEmpty();
                var fixtureToOpen = this.TestFixtures.FirstOrDefault(f => f.FixtureId.Equals(fixtureId)) ?? 
                    throw new ArgumentException($"Test fixture with Id : '{fixtureId}' was not found."); 
                if (fixtureToOpen != null)
                {
                    await OpenTestFixtureAsync(fixtureToOpen);                   
                }                   
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while trying to open test fixture with Id : '{0}'", fixtureId);
                await notificationManager.ShowErrorNotificationAsync(ex);
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
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(CloseTestFixtureAsync), ActivityKind.Internal))
            {
                try
                {
                    activity?.SetTag("Fixture", fixtureVM.DisplayName);
                    if (fixtureVM.IsOpenForEdit)
                    {
                        foreach (var testCase in fixtureVM.Tests)
                        {
                            await CloseTestCaseAsync(testCase, autoSave);
                        }

                        if (autoSave)
                        {
                            await SaveTestFixtureDataAsync(fixtureVM);
                        }

                        CleanUpScriptEditor();
                        RemoveFromEditor();

                        await this.TestRunner.TryCloseTestFixture(fixtureVM.TestFixture);
                        fixtureVM.TestFixtureEntity = null;
                        fixtureVM.IsOpenForEdit = false;

                        logger.Information("Test fixture : '{0}' was closed.", fixtureVM.DisplayName);

                        void CleanUpScriptEditor()
                        {
                            if (!fixtureVM.OpenForExecute)
                            {
                                var fixtureEntityManager = fixtureVM.TestFixtureEntity.EntityManager;
                                IScriptEditorFactory scriptEditorFactory = fixtureEntityManager.GetServiceOfType<IScriptEditorFactory>();
                                scriptEditorFactory.RemoveProject(fixtureVM.FixtureId);
                                fixtureVM.TestFixtureEntity.DisposeEditors();
                            }
                        }

                        void RemoveFromEditor()
                        {
                            if (!fixtureVM.OpenForExecute)
                            {
                                this.componentViewBuilder.CloseTestFixture(fixtureVM.TestFixture);
                                this.eventAggregator.PublishOnUIThreadAsync(new TestEntityRemovedEventArgs(fixtureVM.TestFixtureEntity));
                            }
                        }
                    }
                    NotifyOfPropertyChange(nameof(CanSaveAll));
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error while trying to close test fixture : '{0}'", fixtureVM?.DisplayName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }             
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
                Guard.Argument(fixtureId, nameof(fixtureId)).NotNull().NotEmpty();
                var fixtureToClose = this.TestFixtures.FirstOrDefault(f => f.FixtureId.Equals(fixtureId)) ??
                    throw new ArgumentException($"Test fixture with Id : '{fixtureId}' was not found.");
                if (fixtureToClose != null)
                {
                    await CloseTestFixtureAsync(fixtureToClose, true);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while trying to close test fixture with Id : '{0}'", fixtureId);
                await notificationManager.ShowErrorNotificationAsync(ex);
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
                Guard.Argument(fixtureVM, nameof(fixtureVM)).NotNull();
                if (fixtureVM.IsOpenForEdit)
                {
                    var fixtureEntityManager = fixtureVM.TestFixtureEntity.EntityManager;
                    IScriptEditorFactory scriptEditorFactory = fixtureEntityManager.GetServiceOfType<IScriptEditorFactory>();
                    using (IScriptEditorScreen scriptEditorScreen = scriptEditorFactory.CreateScriptEditorScreen())
                    {
                        scriptEditorScreen.OpenDocument(fixtureVM.ScriptFile, fixtureVM.FixtureId, string.Empty);
                        var result = await this.windowManager.ShowDialogAsync(scriptEditorScreen);
                        if (result.HasValue && result.Value)
                        {
                            logger.Information("Script file : '{0}' was edited for test fixture : '{1}'.", fixtureVM.ScriptFile, fixtureVM.DisplayName);
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
                logger.Error(ex, "Error while trying to edit script file for test fixture : '{0}'", fixtureVM?.DisplayName);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }

        /// <summary>
        /// Save data files belonging to the TestFixture
        /// </summary>
        /// <param name="fixtureVM"></param>
        /// <returns></returns>
        public async Task SaveTestFixtureDataAsync(TestFixtureViewModel fixtureVM)
        {
            Guard.Argument(fixtureVM, nameof(fixtureVM)).NotNull();
            //Remove the test case entities before saving test fixture entity
            var testCaseEntities = fixtureVM.TestFixtureEntity.GetComponentsOfType<TestCaseEntity>(SearchScope.Descendants);
            foreach (var testEntity in testCaseEntities)
            {
                //we don't want to dispose test fixture entity and/or clear out it's entity manager as we need to restore it back soon.
                testEntity.Parent.RemoveComponent(testEntity, false);
            }

            if(fixtureVM.IsDirty)
            {
                await this.testFixtureManager.UpdateTestFixtureAsync(fixtureVM.TestFixture);
                fixtureVM.IsDirty = false;               
            }
            await this.testFixtureManager.SaveFixtureDataAsync(fixtureVM.TestFixture);

            //Add back the test cases that were removed earlier
            foreach (var testEntity in testCaseEntities)
            {
                fixtureVM.TestFixtureEntity.AddComponent(testEntity);
            }           
        }

        #endregion Test Fixture

        #region Test Case      

        /// <summary>
        /// Add a new TestCase to specified TestFixture
        /// </summary>
        /// <param name="fixtureVM"></param>
        /// <returns></returns>
        public async Task AddTestCaseAsync(TestFixtureViewModel fixtureVM)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(AddTestCaseAsync), ActivityKind.Internal))
            {
                try
                {
                    Guard.Argument(fixtureVM, nameof(fixtureVM)).NotNull();
                    TestCase testCase = new TestCase()
                    {
                        FixtureId = fixtureVM.FixtureId,
                        DisplayName = $"Test#{fixtureVM.Tests.Count() + 1}",
                        Order = fixtureVM.Tests.Count() + 1,
                        TestCaseEntity = new TestCaseEntity() { Name = $"Test#{fixtureVM.Tests.Count() + 1}" }
                    };

                    var testCaseEditor = new EditTestCaseViewModel(testCase, fixtureVM.Tests.Select(s => s.DisplayName));
                    bool? result = await this.windowManager.ShowDialogAsync(testCaseEditor);
                    if (result.HasValue && result.Value)
                    {
                        TestCaseViewModel testCaseVM = new TestCaseViewModel(testCase)
                        {
                            PostDelay = applicationSettings.PostDelay,
                            DelayFactor = applicationSettings.DelayFactor
                        };

                        await this.testCaseManager.AddTestCaseAsync(testCase);
                        fixtureVM.Tests.Add(testCaseVM);
                        activity?.SetTag("TestCase", testCase.DisplayName);
                        logger.Information("Added new TestCase {0} to Fixture {1}", testCase.DisplayName, fixtureVM.DisplayName);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error while trying to add new test cae");
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }
           
        }

        /// <summary>
        /// Edit the details of a TestCase
        /// </summary>
        /// <param name="testCaseVM"></param>
        /// <returns></returns>
        public async Task EditTestCaseAsync(TestCaseViewModel testCaseVM)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(EditTestCaseAsync), ActivityKind.Internal))
            {
                try
                {
                    Guard.Argument(testCaseVM, nameof(testCaseVM)).NotNull();
                    activity?.SetTag("TestCase", testCaseVM.DisplayName);
                    var parentFixture = TestFixtures.First(t => t.FixtureId.Equals(testCaseVM.FixtureId));
                    var existingTestCases = parentFixture.Tests.Except(new[] { testCaseVM }).Select(s => s.DisplayName);
                    var testCaseEditor = new EditTestCaseViewModel(testCaseVM.TestCase, existingTestCases);
                    bool? result = await this.windowManager.ShowDialogAsync(testCaseEditor);
                    if (result.HasValue && result.Value)
                    {
                        testCaseVM.Refresh();
                        if (testCaseVM.IsOpenForEdit)
                        {
                            testCaseVM.TestCaseEntity.Name = testCaseVM.DisplayName;
                        }
                        await this.testCaseManager.UpdateTestCaseAsync(testCaseVM.TestCase);
                        testCaseVM.IsDirty = false;
                        logger.Information("Test Case : '{0}' was edited.", testCaseVM.DisplayName);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error while trying to edit Test Case : '{0}'", testCaseVM?.DisplayName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }          
        }

        /// <summary>
        /// Delete an existing TestCase
        /// </summary>
        /// <param name="testCaseVM"></param>
        public async void DeleteTestCase(TestCaseViewModel testCaseVM)
        {
            Guard.Argument(testCaseVM, nameof(testCaseVM)).NotNull();
            if (testCaseVM.IsOpenForEdit)
            {
                MessageBox.Show("Test case is open for edit. An open test case can't be deleted.", "Delete Test Case", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(DeleteTestCase), ActivityKind.Internal))
            {
                try
                {          
                    activity?.SetTag("TestCase", testCaseVM.DisplayName);

                    TestFixtureViewModel testFixtureVM = this.TestFixtures.FirstOrDefault(c => c.FixtureId.Equals(testCaseVM.FixtureId));
                    if (testFixtureVM != null)
                    {
                        MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this test", "Confirm Delete", MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                        {
                            await this.testCaseManager.DeleteTestCaseAsync(testCaseVM.TestCase);
                            testFixtureVM.Tests.Remove(testCaseVM);
                            logger.Information("Deleted Test Case : '{0}' from fixture : '{1}'", testCaseVM.DisplayName, testFixtureVM.DisplayName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error while trying to delete Test Case : '{0}'", testCaseVM?.DisplayName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }             
        }

        /// <summary>
        /// Open a TestCase to be edited on automation editor
        /// </summary>
        /// <param name="testCaseVM"></param>
        /// <returns></returns>
        public async Task OpenTestCaseAsync(TestCaseViewModel testCaseVM)
        {
            if (!testCaseVM.CanOpenForEdit)
            {
                logger.Information("Test Case : '{0}' is already open for edit", testCaseVM.DisplayName);
                return;
            }

            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(OpenTestCaseAsync), ActivityKind.Internal))
            {
                try
                {
                    Guard.Argument(testCaseVM, nameof(testCaseVM)).NotNull();
                    activity?.SetTag("TestCase", testCaseVM.DisplayName);
                   
                    await this.testCaseManager.DownloadTestDataAsync(testCaseVM.TestCase);

                    var testFiles = this.fileSystem.GetTestCaseFiles(testCaseVM.TestCase);
                    testCaseVM.TestCaseEntity = this.projectManager.Load<Entity>(testFiles.ProcessFile);
                    testCaseVM.TestCaseEntity.Name = testCaseVM.DisplayName;
                    testCaseVM.TestCaseEntity.Tag = testCaseVM.TestCaseId;

                    var parentFixture = this.TestFixtures.First(f => f.FixtureId.Equals(testCaseVM.FixtureId));
                    if (!parentFixture.IsOpenForEdit)
                    {
                        await this.OpenTestFixtureAsync(parentFixture);
                    }

                    if (await this.TestRunner.TryOpenTestCase(parentFixture.TestFixture, testCaseVM.TestCase))
                    {
                        OpenForEdit();
                        SetupScriptEditor();
                        testCaseVM.IsOpenForEdit = true;
                        NotifyOfPropertyChange(nameof(CanSaveAll));
                        logger.Information("Test Case : '{0}' is open for edit now", testCaseVM.DisplayName);
                    }
                    else
                    {
                        logger.Warning("Failed to open Test Case : '{0}' for edit.", testCaseVM.DisplayName);
                    }

                    void OpenForEdit()
                    {
                        if (!testCaseVM.OpenForExecute)
                        {
                            this.componentViewBuilder.OpenTestCase(testCaseVM.TestCase);
                        }
                    }

                    void SetupScriptEditor()
                    {
                        if (!testCaseVM.OpenForExecute)
                        {
                            var testEntityManager = testCaseVM.TestCase.TestCaseEntity.EntityManager;
                            IScriptEditorFactory editorFactory = testEntityManager.GetServiceOfType<IScriptEditorFactory>();

                            //Add the test script project and script file to script editor
                            editorFactory.AddProject(testCaseVM.TestCaseId, new string[] { parentFixture.FixtureId }, testEntityManager.Arguments.GetType());
                            string scriptFileContent = this.fileSystem.ReadAllText(testFiles.ScriptFile);
                            editorFactory.AddDocument(testCaseVM.ScriptFile, testCaseVM.TestCaseId, scriptFileContent);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error while trying to open Test Case : '{0}'", testCaseVM?.DisplayName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
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
                Guard.Argument(testCaseId, nameof(testCaseId)).NotNull().NotEmpty();
                foreach (var testFixture in this.TestFixtures)
                {
                    var targetTestCase = testFixture.Tests.FirstOrDefault(t => t.TestCaseId.Equals(testCaseId)) ??
                        throw new ArgumentException($"Test case with Id : '{testCaseId}' was not found.");
                    if (targetTestCase != null)
                    {
                        await OpenTestCaseAsync(targetTestCase);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while trying to open test case with Id : '{0}'", testCaseId);
                await notificationManager.ShowErrorNotificationAsync(ex);
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
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(CloseTestCaseAsync), ActivityKind.Internal))
            {
                try
                {
                    Guard.Argument(testCaseVM, nameof(testCaseVM)).NotNull();
                    activity?.SetTag("TestCase", testCaseVM.DisplayName);
                    if (testCaseVM.IsOpenForEdit)
                    {
                        if (autoSave)
                        {
                            await SaveTestCaseDataAsync(testCaseVM);
                        }
                        var parentFixture = this.TestFixtures.First(f => f.FixtureId.Equals(testCaseVM.FixtureId));

                        CleanUpScriptEditor();
                        RemoveFromEditor();

                        await this.TestRunner.TryCloseTestCase(parentFixture.TestFixture, testCaseVM.TestCase);
                        testCaseVM.TestCaseEntity = null;
                        testCaseVM.IsOpenForEdit = false;

                        logger.Information("Test Case : '{0}' was closed", testCaseVM.DisplayName);

                        void RemoveFromEditor()
                        {
                            if (!testCaseVM.OpenForExecute)
                            {
                                this.componentViewBuilder.CloseTestCase(testCaseVM.TestCase);
                                this.eventAggregator.PublishOnUIThreadAsync(new TestEntityRemovedEventArgs(testCaseVM.TestCaseEntity));
                            }
                        }

                        void CleanUpScriptEditor()
                        {
                            if (!testCaseVM.OpenForExecute)
                            {
                                //Remove the script editor project that was added while opening the test case
                                var entityManager = testCaseVM.TestCaseEntity.EntityManager;
                                var scriptEditorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
                                scriptEditorFactory.RemoveProject(testCaseVM.TestCaseId);
                                testCaseVM.TestCaseEntity.DisposeEditors();
                            }
                        }

                    }
                    NotifyOfPropertyChange(nameof(CanSaveAll));
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error while trying to close Test Case : '{0}'", testCaseVM?.DisplayName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
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
                Guard.Argument(testCaseId, nameof(testCaseId)).NotNull().NotEmpty();
                foreach (var fixture in this.TestFixtures)
                {
                    if (!fixture.IsOpenForEdit)
                    {
                        continue;
                    }
                    foreach (var test in fixture.Tests)
                    {
                        if (test.TestCaseId.Equals(testCaseId))
                        {
                            await CloseTestCaseAsync(test, true);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while trying to close test case with Id : '{0}'", testCaseId);
                await notificationManager.ShowErrorNotificationAsync(ex);
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
                Guard.Argument(testCaseVM, nameof(testCaseVM)).NotNull();
                if (testCaseVM.IsOpenForEdit)
                {
                    var entityManager = testCaseVM.TestCaseEntity.EntityManager;
                    var scriptEditorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
                    using (IScriptEditorScreen scriptEditorScreen = scriptEditorFactory.CreateScriptEditorScreen())
                    {
                        scriptEditorScreen.OpenDocument(testCaseVM.ScriptFile, testCaseVM.TestCaseId, string.Empty);
                        var result = await this.windowManager.ShowDialogAsync(scriptEditorScreen);
                        if (result.HasValue && result.Value)
                        {
                            logger.Information("Script file for Test Case : '{0}' was edited.", testCaseVM.ScriptFile);                          
                            var scriptEngine = entityManager.GetScriptEngine();
                            scriptEngine.ClearState();
                            var parentFixture = this.TestFixtures.First(f => f.FixtureId.Equals(testCaseVM.FixtureId));
                            await scriptEngine.ExecuteFileAsync(parentFixture.ScriptFile);
                            await scriptEngine.ExecuteFileAsync(testCaseVM.ScriptFile);                            
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while trying to edit script file for Test Case : '{0}'", testCaseVM?.DisplayName);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }

        }

        /// <summary>
        /// Save data files belonging to the TestFixture
        /// </summary>
        /// <param name="fixtureVM"></param>
        /// <returns></returns>
        public async Task SaveTestCaseDataAsync(TestCaseViewModel testCaseVM)
        {
            if(testCaseVM.IsDirty)
            {
                await this.testCaseManager.UpdateTestCaseAsync(testCaseVM.TestCase);
                testCaseVM.IsDirty = false;                
            }
            await this.testCaseManager.SaveTestDataAsync(testCaseVM.TestCase);           
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
        /// </summary>
        public async Task SaveAll()
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(SaveAll), ActivityKind.Internal))
            {
                foreach (var fixture in this.TestFixtures)
                {
                    try
                    {
                        if (fixture.IsOpenForEdit)
                        {
                            await SaveTestFixtureDataAsync(fixture);
                            foreach (var testCase in fixture.Tests)
                            {
                                if (testCase.IsOpenForEdit)
                                {
                                    await SaveTestCaseDataAsync(testCase);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Error while trying to save open fixtures and tests.");
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await notificationManager.ShowErrorNotificationAsync(ex);
                    }
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

        bool isExecutionInProgress;
        public bool IsExecutionInProgress
        {
            get => isExecutionInProgress;
            set
            {
                isExecutionInProgress = value;
                NotifyOfPropertyChange(() => CanRunTests);
                NotifyOfPropertyChange(() => CanTearDownEnvironment);
            }
        }

        /// <summary>
        /// Guard method to check whether tests can be executed
        /// </summary>
        public bool CanRunTests
        {
            get => IsSetupComplete && !isExecutionInProgress;
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
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(SetUpEnvironmentAsync), ActivityKind.Internal))
            {
                try
                {
                    await this.TestRunner.SetUpEnvironment();
                    if (this.TestRunner.CanRunTests)
                    {
                        this.IsSetupComplete = true;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error while trying to set up environment.");
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }           
        }

        /// <summary>
        /// Guard method to check whether environment tear down can be done
        /// </summary>
        public bool CanTearDownEnvironment
        {
            get => isSetupComplete  && !isExecutionInProgress;
        }

        /// <summary>
        /// Execute the OneTimeTearDownEntity for the Automation process
        /// </summary>
        /// <returns></returns>
        public async Task TearDownEnvironmentAsync()
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(TearDownEnvironmentAsync), ActivityKind.Internal))
            {
                try
                {
                    await this.TestRunner.TearDownEnvironment();
                    this.IsSetupComplete = false;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error while trying to tear down environment.");
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }             
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
            Task runTestCasesTask = new Task(async () =>
            {
                using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(RunSelected), ActivityKind.Internal))
                {
                    try
                    {
                        logger.Information("------------------------------------------------------");
                        logger.Information("Start execution of selected test case");

                        this.IsExecutionInProgress = true;
                        foreach (var testFixture in this.TestFixtures)
                        {
                            foreach (var test in testFixture.Tests)
                            {
                                if (test.IsSelected)
                                {
                                    activity?.SetTag("TestCase", test.DisplayName);
                                    System.Action clearTestResults = () => test.TestResults.Clear();
                                    platformProvider.OnUIThread(clearTestResults);

                                    //open fixture if not already open
                                    bool isFixtureAlreadyOpenForEdit = testFixture.IsOpenForEdit;
                                    if (!testFixture.IsOpenForEdit)
                                    {
                                        testFixture.OpenForExecute = true;
                                        await OpenTestFixtureAsync(testFixture);
                                        testFixture.OpenForExecute = false;
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
                        logger.Error(ex, "Error while trying to run selected test case");
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await notificationManager.ShowErrorNotificationAsync(ex);
                    }
                    finally
                    {
                        this.IsExecutionInProgress = false;
                        logger.Information("Completed execution of selected test case");
                        logger.Information("------------------------------------------------------");
                    }
                }                
            });
            runTestCasesTask.Start();
            await runTestCasesTask;       
        }

        /// <summary>
        /// Run all the available TestCases
        /// </summary>
        public async Task RunAll()
        {           
            isCancellationRequested = false;
            CanAbort = true;
            Task runTestCasesTask = new Task(async () =>
            {
                using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(RunAll), ActivityKind.Internal))
                {
                    try
                    {
                        logger.Information("------------------------------------------------------");
                        logger.Information("Start execution of all test cases");
                        System.Action clearTestResults = () =>
                        {
                            var tests = this.TestFixtures.SelectMany(t => t.Tests);
                            foreach (var test in tests)
                            {
                                test.TestResults.Clear();
                            }
                        };
                        platformProvider.OnUIThread(clearTestResults);

                        this.IsExecutionInProgress = true;
                        foreach (var testFixture in this.TestFixtures.OrderBy(t => t.Order).ThenBy(t => t.DisplayName))
                        {
                            //open fixture if not already open
                            bool isFixtureAlreadyOpenForEdit = testFixture.IsOpenForEdit;
                            if (!testFixture.IsOpenForEdit)
                            {
                                testFixture.OpenForExecute = true;
                                await OpenTestFixtureAsync(testFixture);
                                testFixture.OpenForExecute = false;
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
                        logger.Error(ex, "Error while trying to run test cases");
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await notificationManager.ShowErrorNotificationAsync(ex);
                    }
                    finally
                    {
                        this.IsExecutionInProgress = false;
                        logger.Information("Completed execution of all test cases");
                        logger.Information("------------------------------------------------------");
                    }
                }
             
            });
            runTestCasesTask.Start();
            await runTestCasesTask;
           
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
                    testCaseVM.OpenForExecute = true;
                    await OpenTestCaseAsync(testCaseVM);
                    testCaseVM.OpenForExecute = false;
                }             

                var parentFixture = this.TestFixtures.First(f => f.FixtureId.Equals(testCaseVM.FixtureId));
                await foreach (var result in this.TestRunner.RunTestAsync(parentFixture.TestFixture, testCaseVM.TestCase))
                {
                    System.Action addTestResult = () => testCaseVM.TestResults.Add(result);
                    platformProvider.OnUIThread(addTestResult);
                }

                return await Task.FromResult(true);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while trying to run test case : '{0}'", testCaseVM?.DisplayName);
                await notificationManager.ShowErrorNotificationAsync(ex);
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
        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            if(!isInitialized)
            {
                await LoadFixturesAsync();
            }
            await base.OnInitializeAsync(cancellationToken);
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

        /// <summary>
        /// Associate usage of a prefab with fixture or test case to which it is added
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>s
        public async Task HandleAsync(PrefabAddedEventArgs message, CancellationToken cancellationToken)
        {            
            foreach(var fixture in this.TestFixtures)
            {
                if(fixture.FixtureId.Equals(message.AddedTo))
                {
                    fixture.AddPrefabUsage(message.PrefabId);                
                    return;
                }
                foreach(var testCase in fixture.Tests)
                {
                    if(testCase.TestCaseId.Equals(message.AddedTo))
                    {
                        testCase.AddPrefabUsage(message.PrefabId);                       
                        return;
                    }
                }
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Remove usage of prefab from fixture or test case
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task HandleAsync(PrefabRemovedEventArgs message, CancellationToken cancellationToken)
        {
            foreach (var fixture in this.TestFixtures)
            {
                if (fixture.FixtureId.Equals(message.RemovedFrom))
                {
                    fixture.RemovePrefabUsage(message.PrefabId);                   
                    return;
                }
                foreach (var testCase in fixture.Tests)
                {
                    if (testCase.TestCaseId.Equals(message.RemovedFrom))
                    {
                        testCase.RemovePrefabUsage(message.PrefabId);                        
                        return;
                    }
                }
            }
            await Task.CompletedTask;
      
        }

        /// <summary>
        /// Associate usage of a control with fixture or test case to which it is added
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task HandleAsync(ControlAddedEventArgs message, CancellationToken cancellationToken)
        {
            if(!string.IsNullOrEmpty(message.AddedTo))
            {
                foreach (var fixture in this.TestFixtures)
                {
                    if (fixture.FixtureId.Equals(message.AddedTo))
                    {
                        fixture.AddControlUsage(message.Control.ControlId);                      
                        return;
                    }
                    foreach (var testCase in fixture.Tests)
                    {
                        if (testCase.TestCaseId.Equals(message.AddedTo))
                        {
                            testCase.AddControlUsage(message.Control.ControlId);                          
                            return;
                        }
                    }
                }
            }            
            await Task.CompletedTask;
        }


        /// <summary>
        /// Remove usage of a control from fixture or test case
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task HandleAsync(ControlRemovedEventArgs message, CancellationToken cancellationToken)
        {
            foreach (var fixture in this.TestFixtures)
            {
                if (fixture.FixtureId.Equals(message.RemovedFrom))
                {
                    fixture.RemoveControlUsage(message.ControlId);                   
                    return;
                }
                foreach (var testCase in fixture.Tests)
                {
                    if (testCase.TestCaseId.Equals(message.RemovedFrom))
                    {
                        testCase.RemoveControlUsage(message.ControlId);                       
                        return;
                    }
                }
            }
            await Task.CompletedTask;
        }

        #endregion Notifications
    }
}
