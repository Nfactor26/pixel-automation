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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Pixel.Automation.TestData.Repository.ViewModels
{
    /// <summary>
    /// TestDataRepository allows creating different data sources which can be used in a test. TestDataRepository belongs to a workspace.
    /// </summary>
    public class TestDataRepositoryViewModel : Screen, ITestDataRepository, IHandle<ShowTestDataSourceNotification>
    {
        private readonly ILogger logger = Log.ForContext<TestDataRepositoryViewModel>();

        private readonly IProjectFileSystem projectFileSystem;
        private readonly IScriptEditorFactory scriptEditorFactory;
        private readonly ISerializer serializer;
        private readonly IArgumentTypeBrowserFactory typeBrowserFactory;
        private readonly IWindowManager windowManager;
        private readonly INotificationManager notificationManager;
        private readonly IEventAggregator eventAggregator;
        private readonly ITestDataManager testDataManager;

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
        public TestDataRepositoryViewModel(ISerializer serializer, IProjectFileSystem projectFileSystem, IScriptEditorFactory scriptEditorFactory,
            IWindowManager windowManager, INotificationManager notificationManager, IEventAggregator eventAggregator, IArgumentTypeBrowserFactory typeBrowserFactory, IProjectAssetsDataManager projectAssetsDataManager)
        {
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.projectFileSystem = Guard.Argument(projectFileSystem, nameof(projectFileSystem)).NotNull().Value;
            this.scriptEditorFactory = Guard.Argument(scriptEditorFactory, nameof(scriptEditorFactory)).NotNull().Value;
            this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;
            this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;
            this.typeBrowserFactory = Guard.Argument(typeBrowserFactory, nameof(typeBrowserFactory)).NotNull().Value;
            this.eventAggregator = Guard.Argument(eventAggregator, nameof(eventAggregator)).NotNull().Value;
            this.testDataManager = Guard.Argument(projectAssetsDataManager, nameof(projectAssetsDataManager)).NotNull().Value;

            this.eventAggregator.SubscribeOnPublishedThread(this);
            CreateCollectionView();
        }

        /// <summary>
        /// Download and load the available TestDataSources
        /// </summary>
        private async Task LoadDataSourcesAsync()
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(LoadDataSourcesAsync), ActivityKind.Internal))
            {
                await this.testDataManager.DownloadAllTestDataSourcesAsync();
                foreach (var testDataSource in this.projectFileSystem.GetTestDataSources())
                {
                    if (!testDataSource.IsDeleted)
                    {
                        this.TestDataSourceCollection.Add(testDataSource);
                    }
                }
            }
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
                    await this.testDataManager.AddTestDataSourceAsync(newTestDataSource.TestDataSource);
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

        private async Task CreateEmptyDataSourceAsync()
        {
            string testDataId = Guid.NewGuid().ToString();
            var testDataSource = new TestDataSource()
            {
                DataSourceId = testDataId,
                Name = "EmptyDataSource",
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

            await this.testDataManager.AddTestDataSourceAsync(testDataSource);
            await this.testDataManager.SaveTestDataSourceDataAsync(testDataSource);

        }

        #endregion Create Test Data Source       

        #region Edit Test Data Source

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
                        logger.Information("Data source : '{0}' was deleted ", testDataSource.Name);

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

        #endregion Edit Data Source

        #region life cycle

        /// <summary>
        /// Called just before view is activiated for the first time.
        /// Available TestDataSource are loaded from local storage during initialization.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await LoadDataSourcesAsync();
            if (this.TestDataSourceCollection.Count == 0)
            {
                await CreateEmptyDataSourceAsync();
                foreach (var testDataSource in this.projectFileSystem.GetTestDataSources())
                {
                    this.TestDataSourceCollection.Add(testDataSource);
                }
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
