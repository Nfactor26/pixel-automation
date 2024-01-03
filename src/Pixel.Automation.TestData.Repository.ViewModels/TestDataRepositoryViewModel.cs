using Caliburn.Micro;
using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Notifications;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Pixel.Automation.TestData.Repository.ViewModels
{
    /// <summary>
    /// TestDataRepository allows creating different data sources which can be used in a test. TestDataRepository belongs to a workspace.
    /// </summary>
    public class TestDataRepositoryViewModel : Screen, ITestDataRepository, IHandle<ShowTestDataSourceNotification>
    {
        private readonly ILogger logger = Log.ForContext<TestDataRepositoryViewModel>();

        private readonly IProjectManager projectManager;
        private readonly IProjectFileSystem projectFileSystem;
        private readonly IScriptEditorFactory scriptEditorFactory;
        private readonly ISerializer serializer;
        private readonly IArgumentTypeBrowserFactory typeBrowserFactory;
        private readonly IWindowManager windowManager;
        private readonly INotificationManager notificationManager;
        private readonly IEventAggregator eventAggregator;
        private readonly ITestDataManager testDataManager;

        private Dictionary<string, List<TestDataSource>> CachedCollection = new Dictionary<string, List<TestDataSource>>();

        /// <summary>
        /// Collection of TestDataSource available for the associated automation project
        /// </summary>
        public BindableCollection<TestDataSource> TestDataSourceCollection { get; } = new BindableCollection<TestDataSource>();

        private TestDataSource selectedTestDataSource;
        /// <summary>
        /// Selected TestDataSource on the view
        /// </summary>
        public TestDataSource SelectedTestDataSource
        {
            get => this.selectedTestDataSource;
            set
            {
                this.selectedTestDataSource = value;
                NotifyOfPropertyChange(() => SelectedTestDataSource);
            }
        }

        #region Constructor

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="projectFileSystem"></param>
        /// <param name="scriptEditorFactory"></param>
        /// <param name="windowManager"></param>
        /// <param name="typeProvider"></param>
        /// <param name="typeBrowserFactory"></param>
        public TestDataRepositoryViewModel(ISerializer serializer, IAutomationProjectManager projectManager, IProjectFileSystem projectFileSystem, IScriptEditorFactory scriptEditorFactory,
            IWindowManager windowManager, INotificationManager notificationManager, IEventAggregator eventAggregator, IArgumentTypeBrowserFactory typeBrowserFactory,
            IProjectAssetsDataManager projectAssetsDataManager)
        {
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.projectManager = Guard.Argument(projectManager, nameof(projectManager)).NotNull().Value;
            this.projectFileSystem = Guard.Argument(projectFileSystem, nameof(projectFileSystem)).NotNull().Value;
            this.scriptEditorFactory = Guard.Argument(scriptEditorFactory, nameof(scriptEditorFactory)).NotNull().Value;
            this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;
            this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;
            this.typeBrowserFactory = Guard.Argument(typeBrowserFactory, nameof(typeBrowserFactory)).NotNull().Value;
            this.eventAggregator = Guard.Argument(eventAggregator, nameof(eventAggregator)).NotNull().Value;
            this.testDataManager = Guard.Argument(projectAssetsDataManager, nameof(projectAssetsDataManager)).NotNull().Value;           
            this.eventAggregator.SubscribeOnPublishedThread(this);
            CreateCollectionView();
            this.projectManager.ProjectLoaded += OnProjectLoaded;
        }

        internal async Task OnProjectLoaded(object sender, ProjectLoadedEventArgs e)
        {
            await DownloadDataSourcesAsync();
            var referenceManager = this.projectManager.GetReferenceManager();
            this.Groups.AddRange(referenceManager.GetTestDataSourceGroups());
            //Create an empty data source if no data source exist in any of the groups
            if (!referenceManager.GetTestDataSourceGroups().Any(t => referenceManager.GetTestDataSources(t).Count() > 0))
            {
                await CreateEmptyDataSourceAsync(this.Groups.First());
            }
            this.SelectedGroup = this.Groups.First();
            this.projectManager.ProjectLoaded -= OnProjectLoaded;
        }

       
        #endregion Constructor

        #region Filter 


        string filterText = string.Empty;
        /// <summary>
        /// Controls visibility of TestDataSource displayed on view
        /// </summary>
        public string FilterText
        {
            get
            {
                return filterText;
            }
            set
            {
                filterText = value;
                NotifyOfPropertyChange(() => FilterText);
                var view = CollectionViewSource.GetDefaultView(TestDataSourceCollection);
                view.Refresh();
                NotifyOfPropertyChange(() => TestDataSourceCollection);

            }
        }

        /// <summary>
        /// Setup the collection view with grouping and sorting
        /// </summary>
        private void CreateCollectionView()
        {
            var groupedItems = CollectionViewSource.GetDefaultView(TestDataSourceCollection);
            groupedItems.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TestDataSource.Name)));
            groupedItems.SortDescriptions.Add(new SortDescription(nameof(TestDataSource.Name), ListSortDirection.Ascending));
            groupedItems.Filter = new Predicate<object>((a) =>
            {
                var testDataSource = a as TestDataSource;
                return testDataSource.Name.ToLower().Contains(this.filterText.ToLower()) || testDataSource.DataSourceId.ToLower().Contains(this.filterText.ToLower());
            });
        }

        #endregion Filter         

        #region Test Data Groups

        /// <summary>
        /// Collection of available test data source groups
        /// </summary>
        public BindableCollection<string> Groups { get; private set; } = new();

        private string selectedGroup;
        /// <summary>
        /// Currently selected screen
        /// </summary>
        public string SelectedGroup
        {
            get => selectedGroup;
            set
            {
                if(value == selectedGroup)
                {
                    return;
                }
                selectedGroup = value;
                OnGroupChanged(value);
                NotifyOfPropertyChange();
            }
        }

        void OnGroupChanged(string groupName)
        {
            try
            {
                if(string.IsNullOrEmpty(groupName))
                {
                    this.TestDataSourceCollection.Clear();
                    return;
                }
                if (this.CachedCollection.ContainsKey(groupName))
                {
                    this.TestDataSourceCollection.Clear();
                    this.TestDataSourceCollection.AddRange(this.CachedCollection[groupName]);
                    return;
                }
                List<TestDataSource> dataSourcesForGroup = new();
                this.CachedCollection.Add(groupName, dataSourcesForGroup);
                var dataSourcesInGroup = this.projectManager.GetReferenceManager().GetTestDataSources(groupName);
                foreach (var dataSourceId in dataSourcesInGroup)
                {
                    var dataSource = this.projectFileSystem.GetTestDataSourceById(dataSourceId);
                    if(!dataSource.IsDeleted)
                    {
                        dataSourcesForGroup.Add(dataSource);
                    }
                }
                this.TestDataSourceCollection.Clear();
                this.TestDataSourceCollection.AddRange(this.CachedCollection[groupName]);
                logger.Information("Loaded '{0}' test data sources for group : '{1}'", dataSourcesForGroup.Count(), groupName);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while loading data sources for group {0}", groupName);               
                _ = notificationManager.ShowErrorNotificationAsync($"There was an error while loading data sources for group {groupName}");
            }
        }

        /// <summary>
        /// Create a new group for the test data sources
        /// </summary>
        /// <returns></returns>
        public async Task CreateGroup()
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(CreateGroup), ActivityKind.Internal))
            {
                try
                {
                    var dataSourceGroupViewModel = new NewDataSourceGroupViewModel(this.projectManager.GetReferenceManager(), this.notificationManager);
                    var result = await windowManager.ShowDialogAsync(dataSourceGroupViewModel);
                    if (result.GetValueOrDefault())
                    {                        
                        activity?.SetTag("ScreenName", dataSourceGroupViewModel.GroupName);                  
                        this.Groups.Add(dataSourceGroupViewModel.GroupName);
                        this.SelectedGroup = dataSourceGroupViewModel.GroupName;
                        logger.Information("Group : '{0}' was created for test data source", dataSourceGroupViewModel.GroupName);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while creating new screen");
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }
        }

        /// <summary>
        /// Rename an existing test data source group name
        /// </summary>
        /// <returns></returns>
        public async Task RenameGroup()
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(RenameGroup), ActivityKind.Internal))
            {
                try
                {
                    var renameScreenViewModel = new RenameDataSourceGroupViewModel(this.SelectedGroup, this.projectManager.GetReferenceManager(), this.notificationManager);
                    var result = await windowManager.ShowDialogAsync(renameScreenViewModel);
                    if (result.GetValueOrDefault())
                    {                      
                        activity?.SetTag("CurrentGroupName", renameScreenViewModel.GroupName);
                        activity?.SetTag("NewGroupName", renameScreenViewModel.NewGroupName);                      
                        this.Groups.Add(renameScreenViewModel.NewGroupName);
                        this.SelectedGroup = renameScreenViewModel.NewGroupName;
                        this.Groups.Remove(renameScreenViewModel.GroupName);                      
                        logger.Information("Renamed group {0} to {1} ", renameScreenViewModel.GroupName, renameScreenViewModel.NewGroupName);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while renaming the screen");
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }
        }

        /// <summary>
        /// Move TestDataSource from one group to another
        /// </summary>
        /// <param name="testDataSource"></param>
        /// <returns></returns>
        public async Task MoveToGroup(TestDataSource testDataSource)
        {
            Guard.Argument(testDataSource, nameof(testDataSource)).NotNull();
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(MoveToGroup), ActivityKind.Internal))
            {
                try
                {
                    activity?.SetTag("TestDataSourceName", testDataSource.Name);
                    var moveToGroupViewModel = new MoveToGroupViewModel(testDataSource, this.Groups, this.SelectedGroup, this.projectManager.GetReferenceManager(), this.notificationManager);
                    var result = await windowManager.ShowDialogAsync(moveToGroupViewModel);
                    if (result.GetValueOrDefault())
                    {                     
                        this.TestDataSourceCollection.Remove(testDataSource);
                        this.CachedCollection[this.SelectedGroup].Remove(testDataSource);
                        if(!this.CachedCollection.ContainsKey(moveToGroupViewModel.SelectedGroup))
                        {
                            this.CachedCollection.Add(moveToGroupViewModel.SelectedGroup, new());
                        }
                        this.CachedCollection[moveToGroupViewModel.SelectedGroup].Add(testDataSource);
                        logger.Information("Moved test data source : {0} from group {1} to {2}", testDataSource.Name, this.SelectedGroup, moveToGroupViewModel.SelectedGroup);
                        await notificationManager.ShowSuccessNotificationAsync($"Data Source : '{testDataSource.Name}' was moved to group : '{moveToGroupViewModel.SelectedGroup}'");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while moving data source : '{0}' to another group", testDataSource?.Name);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }
        }
        
        #endregion Test Data Groups

        #region Create Test Data Source

        ///<inheritdoc/>
        public async Task CreateCodedTestDataSource()
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(CreateCodedTestDataSource), ActivityKind.Internal))
            {
                await CreateDataSource(DataSource.Code);
            }
        }

        ///<inheritdoc/>
        public async Task CreateCsvTestDataSource()
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(CreateCsvTestDataSource), ActivityKind.Internal))
            {
                await CreateDataSource(DataSource.CsvFile);
            }
        }

        private async Task CreateDataSource(DataSource dataSourceType)
        {
            try
            {
                //value types like int, bool , etc. as well as string doesn't make sense for a data source given they can't be accessed.
                //Valid data sources are custom classes defined by the automation project which expose properties that are visible to scripting engine.
                //Hence, we create the type browser with showOnlyCustomTypes = true
                var argumentTypeBrowser = typeBrowserFactory.CreateArgumentTypeBrowser(showOnlyCustomTypes: true);
                TestDataSourceViewModel newTestDataSource = new TestDataSourceViewModel(this.windowManager, this.projectFileSystem, dataSourceType, this.TestDataSourceCollection.Select(t => t.Name) ?? Array.Empty<string>(), argumentTypeBrowser);

                TestDataModelEditorViewModel testDataModelEditor = new TestDataModelEditorViewModel(this.scriptEditorFactory);

                newTestDataSource.NextScreen = testDataModelEditor;
                testDataModelEditor.PreviousScreen = newTestDataSource;

                TestDataSourceBuilderViewModel testDataSourceBuilder = new TestDataSourceBuilderViewModel(new IStagedScreen[] { newTestDataSource, testDataModelEditor });

                var result = await windowManager.ShowDialogAsync(testDataSourceBuilder);
                if (result.HasValue && result.Value)
                {
                    await this.testDataManager.AddTestDataSourceAsync(this.SelectedGroup, newTestDataSource.TestDataSource);
                    this.projectManager.GetReferenceManager().AddTestDataSource(this.SelectedGroup, newTestDataSource.DataSourceId);
                    this.CachedCollection[this.SelectedGroup].Add(newTestDataSource.TestDataSource);
                    this.TestDataSourceCollection.Add(newTestDataSource.TestDataSource);
                }
                logger.Information("Data source of type {0} was created", dataSourceType.ToString());

            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to create data source of type {0}", dataSourceType.ToString());
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }

        private async Task CreateEmptyDataSourceAsync(string groupName)
        {
            string testDataId = Guid.NewGuid().ToString();
            var testDataSource = new TestDataSource()
            {
                DataSourceId = testDataId,
                Name = "Empty Data Source",
                ScriptFile = Path.GetRelativePath(this.projectFileSystem.WorkingDirectory, Path.Combine(this.projectFileSystem.TestDataRepository, $"{testDataId}.csx")).Replace("\\", "/"),
                DataSource = DataSource.Code,
                MetaData = new DataSourceConfiguration()
                {
                    TargetTypeName = "EmptyModel"
                }
            };
            serializer.Serialize<TestDataSource>(Path.Combine(this.projectFileSystem.TestDataRepository, $"{testDataId}.dat"), testDataSource);
            StringBuilder scriptTextBuilder = new StringBuilder();
            scriptTextBuilder.Append("using System;");
            scriptTextBuilder.Append(Environment.NewLine);
            scriptTextBuilder.Append("using System.Collections.Generic;");
            scriptTextBuilder.Append(Environment.NewLine);
            scriptTextBuilder.Append("using Pixel.Automation.Core.TestData;");
            scriptTextBuilder.Append(Environment.NewLine);
            scriptTextBuilder.Append(Environment.NewLine);
            scriptTextBuilder.Append("IEnumerable<EmptyModel> GetDataRows()");
            scriptTextBuilder.Append(Environment.NewLine);
            scriptTextBuilder.Append("{");
            scriptTextBuilder.Append(Environment.NewLine);
            scriptTextBuilder.Append("   yield return new EmptyModel();");
            scriptTextBuilder.Append(Environment.NewLine);
            scriptTextBuilder.Append("}");
            File.WriteAllText(Path.Combine(this.projectFileSystem.TestDataRepository, $"{testDataId}.csx"), scriptTextBuilder.ToString());

            await this.testDataManager.AddTestDataSourceAsync(groupName, testDataSource);          
            await this.testDataManager.SaveTestDataSourceDataAsync(testDataSource);
            this.projectManager.GetReferenceManager().AddTestDataSource(groupName, testDataSource.DataSourceId);

        }

        #endregion Create Test Data Source       

        #region Edit Test Data Source

        bool canEdit;
        /// <summary>
        /// Indicates if selected test data source name can be edited
        /// </summary>
        public bool CanEdit
        {
            get
            {
                return canEdit;
            }
            set
            {
                canEdit = value;
                NotifyOfPropertyChange(() => CanEdit);
            }
        }

        /// <summary>
        /// Double click on test data source name to toggle visibility of textbox which can be used to edit the name
        /// </summary>
        /// <param name="targetControl"></param>
        public void ToggleRename(TestDataSource testDataSource)
        {
            if (SelectedTestDataSource == testDataSource)
            {
                CanEdit = !CanEdit;
            }
        }

        /// <summary>
        /// Press enter when in edit mode to apply the changed name of control after control name is edited in text box
        /// </summary>
        /// <param name="context"></param>
        /// <param name="controlToRename"></param>
        public async Task RenameDataSource(ActionExecutionContext context, TestDataSource testDataSource)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(TestDataSource), ActivityKind.Internal))
            {
                string currentName = testDataSource.Name;
                try
                {
                    var keyArgs = context.EventArgs as KeyEventArgs;
                    if (keyArgs != null && keyArgs.Key == Key.Enter)
                    {
                        string newName = (context.Source as System.Windows.Controls.TextBox).Text;
                        if (this.TestDataSourceCollection.Except(new[] { testDataSource }).Any(a => a.Name.Equals(newName)))
                        {
                            return;
                        }
                        activity?.SetTag("CurrentDataSourceName", currentName);
                        activity?.SetTag("NewDataSourceName", newName);
                        testDataSource.Name = newName;
                        CanEdit = false;
                        logger.Information("Data source : '{0}' was renamed to {1}", currentName, newName);
                        await this.testDataManager.UpdateTestDataSourceAsync(testDataSource);
                        await notificationManager.ShowSuccessNotificationAsync($"Data source : '{currentName}' was renamed to : '{newName}'");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while renaming test data source : {0}.", testDataSource.Name);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    testDataSource.Name = currentName;
                    CanEdit = false;
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }
        }

        ///<inheritdoc/>
        public async Task EditDataSource(TestDataSource testDataSource)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(EditDataSource), ActivityKind.Internal))
            {
                try
                {
                    Guard.Argument(testDataSource, nameof(testDataSource)).NotNull();
                    switch (testDataSource.DataSource)
                    {
                        case DataSource.Code:
                            await EditCodedDataSource(testDataSource);
                            break;
                        case DataSource.CsvFile:
                            await EditCsvDataSource(testDataSource);
                            break;
                    }
                    logger.Information("Data source : '{0}' was edited", testDataSource.Name);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to edit data source :'{0}'", testDataSource?.Name);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }
        }

        /// <summary>
        /// show script editor screen to edit the script for Code data source
        /// </summary>
        /// <param name="testDataSource"></param>
        private async Task EditCodedDataSource(TestDataSource testDataSource)
        {
            string projectName = testDataSource.DataSourceId;
            this.scriptEditorFactory.AddProject(projectName, Array.Empty<string>(), typeof(EmptyModel));
            using (var scriptEditor = this.scriptEditorFactory.CreateScriptEditorScreen())
            {
                scriptEditor.OpenDocument(testDataSource.ScriptFile, projectName, string.Empty); //File contents will be fetched from disk         
                var result = await windowManager.ShowDialogAsync(scriptEditor);
                this.scriptEditorFactory.RemoveProject(projectName);
                await this.testDataManager.SaveTestDataSourceDataAsync(testDataSource);
            }

        }

        /// <summary>
        /// Show the TestDataSource screen to edit the details for CSV data source
        /// </summary>
        /// <param name="testDataSource"></param>
        private async Task EditCsvDataSource(TestDataSource testDataSource)
        {
            TestDataSourceViewModel dataSourceViewModel = new TestDataSourceViewModel(this.windowManager, this.projectFileSystem, testDataSource);
            TestDataSourceBuilderViewModel testDataSourceBuilder = new TestDataSourceBuilderViewModel(new IStagedScreen[] { dataSourceViewModel });
            var result = await windowManager.ShowDialogAsync(testDataSourceBuilder);
            if (result.HasValue && result.Value)
            {
                await this.testDataManager.UpdateTestDataSourceAsync(testDataSource);
                await this.testDataManager.SaveTestDataSourceDataAsync(testDataSource);
            }
        }

        /// <summary>
        /// Mark the test data source as deleted
        /// </summary>
        /// <param name="testDataSource"></param>
        public async Task DeleteDataSource(TestDataSource testDataSource)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(DeleteDataSource), ActivityKind.Internal))
            {
                Guard.Argument(testDataSource, nameof(testDataSource)).NotNull();
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete data source", "Confirm Delete", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        await this.testDataManager.DeleteTestDataSourceAsync(testDataSource);
                        this.TestDataSourceCollection.Remove(testDataSource);
                        logger.Information("Data source : '{0}' was deleted", testDataSource.Name);
                        await notificationManager.ShowSuccessNotificationAsync($"Data source : '{testDataSource.Name}' was deleted");

                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "There was an error while trying to delete data source : '{0}'", testDataSource?.Name);
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await notificationManager.ShowErrorNotificationAsync(ex);
                    }
                }
            }         
        }

        /// <summary>
        /// Broadcast a <see cref="TestFilterNotification"/> which is processed by Test data source explorer to filter and show only matching test cases that use
        /// this data source.
        /// </summary>
        /// <param name="testDataSource">TestData source whose usage needs to be shown</param>
        /// <returns></returns>
        public async Task ShowUsage(TestDataSource testDataSource)
        {
            Guard.Argument(testDataSource, nameof(testDataSource)).NotNull();
            await this.eventAggregator.PublishOnUIThreadAsync(new TestFilterNotification("testDataSource", testDataSource.DataSourceId));
        }


        #endregion Edit Data Source

        #region life cycle             

        /// <summary>
        /// Download and load the available TestDataSources
        /// </summary>
        private async Task DownloadDataSourcesAsync()
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(DownloadDataSourcesAsync), ActivityKind.Internal))
            {
                await this.testDataManager.DownloadAllTestDataSourcesAsync();
            }
        }
       
        /// <summary>
        /// Called when the view is deactivated. close will be true when view should be closed as well.
        /// </summary>
        /// <param name="close"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close)
            {
                SelectedTestDataSource = null;
                TestDataSourceCollection.Clear();
            }
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion life cycle

        #region Notifications   

        /// <summary>
        /// Notification handler for <see cref="ShowTestDataSourceNotification"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task HandleAsync(ShowTestDataSourceNotification message, CancellationToken cancellationToken)
        {
            if (message != null)
            {
                this.FilterText = message.TestDataId ?? string.Empty;
            }
            await Task.CompletedTask;
        }

        #endregion Notifications
    }
}
