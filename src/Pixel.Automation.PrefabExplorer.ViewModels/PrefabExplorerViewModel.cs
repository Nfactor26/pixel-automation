using Caliburn.Micro;
using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Notifications.Notfications;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace Pixel.Automation.PrefabExplorer.ViewModels;


public class PrefabExplorerViewModel : Screen, IPrefabExplorer, IHandle<OpenPrefabRequestNotification>, IHandle<ClosePrefabRequestNotification>, IHandle<SavePrefabRequestNotification>
{
    #region Data members

    private readonly ILogger logger = Log.ForContext<PrefabExplorerViewModel>();

    private readonly IProjectFileSystem fileSystem;
    private readonly IProjectManager projectManager;
    private readonly IComponentViewBuilder componentViewBuilder;
    private readonly IEventAggregator eventAggregator;
    private readonly IWindowManager windowManager;
    private readonly INotificationManager notificationManager;   
    private readonly IPrefabManager prefabManager;     
    private readonly IArgumentTypeBrowserFactory argumentTypeBrowserFactory;
    private readonly IEntityManager entityManager;

    /// <summary>
    /// TestRunner allows execution of test cases and setup and tear down of environment
    /// </summary>
    public ITestRunner TestRunner { get; private set; }    

    /// <summary>
    /// A collection of TestFixtures belonging to the Automation Project
    /// </summary>
    public BindableCollection<PrefabViewModel> Prefabs { get; set; } = new BindableCollection<PrefabViewModel>();

    private PrefabViewModel selectedPrefab;
    /// <summary>
    /// Selected TestDataSource on the view
    /// </summary>
    public PrefabViewModel SelectedPrefab
    {
        get => this.selectedPrefab;
        set
        {
            this.selectedPrefab = value;
            NotifyOfPropertyChange(() => SelectedPrefab);
        }
    }

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
            var prefabView = CollectionViewSource.GetDefaultView(Prefabs);
            prefabView.Refresh();
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
    /// <param name="entityManager"></param>
    /// <param name="windowManager"></param>
    /// <param name="applicationSettings"></param>
    public PrefabExplorerViewModel(IEventAggregator eventAggregator, IAutomationProjectManager projectManager,
        IProjectFileSystem fileSystem, IEntityManager entityManager, IProjectAssetsDataManager projectAssetsDataManager,
        IComponentViewBuilder componentViewBuilder, IWindowManager windowManager, INotificationManager notificationManager,
        IArgumentTypeBrowserFactory argumentTypeBrowserFactory)
    {
        this.projectManager = Guard.Argument(projectManager, nameof(projectManager)).NotNull().Value;
        this.componentViewBuilder = Guard.Argument(componentViewBuilder, nameof(componentViewBuilder)).NotNull().Value;
        this.fileSystem = Guard.Argument(fileSystem, nameof(fileSystem)).NotNull().Value;
        this.entityManager = Guard.Argument(entityManager, nameof(entityManager)).NotNull().Value;
        this.prefabManager = Guard.Argument(projectAssetsDataManager, nameof(projectAssetsDataManager)).NotNull().Value;    
        this.eventAggregator = Guard.Argument(eventAggregator, nameof(eventAggregator)).NotNull().Value;
        this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;
        this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;      
        this.argumentTypeBrowserFactory = Guard.Argument(argumentTypeBrowserFactory, nameof(argumentTypeBrowserFactory)).NotNull().Value;      
        this.eventAggregator.SubscribeOnPublishedThread(this);      
        this.projectManager.ProjectLoaded += OnProjectLoaded;

        CreateDefaultView();
    }

    internal async Task OnProjectLoaded(object sender, ProjectLoadedEventArgs e)
    {
        await LoadPrefabsAsync();
        this.projectManager.ProjectLoaded -= OnProjectLoaded;
    }

    /// <summary>
    /// Setup the collection view with grouping and sorting
    /// </summary>
    private void CreateDefaultView()
    {
        var prefabGroupedItems = CollectionViewSource.GetDefaultView(Prefabs);
        prefabGroupedItems.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
        prefabGroupedItems.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));
        prefabGroupedItems.Filter = new Predicate<object>((a) =>
        {
            if (a is PrefabViewModel prefab)
            {
                return prefab.DisplayName.ToLower().Contains(this.filterText.ToLower());
            }
            return true;
        });
    }

    /// <summary>
    /// Download fixtures and test cases and load the fixtures
    /// </summary>
    async Task LoadPrefabsAsync()
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(LoadPrefabsAsync), ActivityKind.Internal))
        {
            try
            {
                await DownloadPrefabsAsync();
                foreach (var prefabsDirectory in Directory.GetDirectories(Path.Combine(this.fileSystem.PrefabsRepository)))
                {                   
                    var prefab = this.fileSystem.LoadFiles<Prefab>(prefabsDirectory).Single();
                    if (prefab.IsDeleted)
                    {
                        logger.Warning("Skip loading Prefab : '{0}' as it is marked deleted", prefab.DisplayName);
                        continue;
                    }
                    PrefabViewModel prefabVM = new PrefabViewModel(prefab);
                    this.Prefabs.Add(prefabVM);
                    activity?.SetTag("Prefab", prefabVM.DisplayName);
                }
               
                logger.Information("Loaded {0} prefabs from local storage for prefab explorer", this.Prefabs.Count());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to load the prefabs");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }
    }

    #endregion Constructor

    #region Prefab   

    /// <summary>
    /// Add a new Prefab
    /// </summary>
    /// <param name="prefabVM"></param>
    /// <returns></returns>
    public async Task AddPrefabAsync()
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(AddPrefabAsync), ActivityKind.Internal))
        {
            try
            {              
                Prefab prefab = new Prefab()
                {                    
                    DisplayName = $"Prefab#{this.Prefabs.Count() + 1}",                  
                    WorkFlowEntity = new PrefabEntity()
                };
               
                var testCaseEditor = new EditPrefabViewModel(prefab, this.Prefabs.Select(s => s.DisplayName), argumentTypeBrowserFactory, windowManager);
                bool? result = await this.windowManager.ShowDialogAsync(testCaseEditor);
                if (result.HasValue && result.Value)
                {
                    prefab.WorkFlowEntity.Name = prefab.DisplayName;
                    PrefabViewModel prefabVM = new PrefabViewModel(prefab);
                    await this.prefabManager.AddPrefabAsync(prefab); 
                    this.Prefabs.Add(prefabVM);
                    activity?.SetTag("Prefab", prefab.DisplayName);
                    logger.Information("Created new Prefab {0}", prefab.DisplayName);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while trying to add new prefab");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }

    }

    /// <summary>
    /// Edit the details of a Prefab
    /// </summary>
    /// <param name="prefabVM"></param>
    /// <returns></returns>
    public async Task EditPrefabAsync(PrefabViewModel prefabVM)
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(EditPrefabAsync), ActivityKind.Internal))
        {
            try
            {
                Guard.Argument(prefabVM, nameof(prefabVM)).NotNull();
                activity?.SetTag("Prefab", prefabVM.DisplayName);
                await RefreshPrefabAsync(prefabVM);             
                var prefabEditor = new EditPrefabViewModel(prefabVM.Prefab, this.Prefabs.Except([prefabVM]).Select(s => s.DisplayName), argumentTypeBrowserFactory, windowManager);
                bool? result = await this.windowManager.ShowDialogAsync(prefabEditor);
                if (result.HasValue && result.Value)
                {
                    prefabVM.Refresh();
                    if (prefabVM.IsOpenForEdit)
                    {
                        prefabVM.WorkflowEntity.Name = prefabVM.DisplayName;
                    }
                    await this.prefabManager.UpdatePrefabAsync(prefabVM.Prefab);
                    prefabVM.IsDirty = false;
                    logger.Information("Prefab : '{0}' was edited.", prefabVM.DisplayName);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while trying to edit Prefab : '{0}'", prefabVM?.DisplayName);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }
    }

    /// <summary>
    /// Delete an existing Prefab
    /// </summary>
    /// <param name="prefabVM"></param>
    public async void DeletePrefab(PrefabViewModel prefabVM)
    {
        Guard.Argument(prefabVM, nameof(prefabVM)).NotNull();
        if (prefabVM.IsOpenForEdit)
        {
            MessageBox.Show("Prefab is open for edit. An open Prefab can't be deleted.", "Delete Prefab", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(DeletePrefab), ActivityKind.Internal))
        {
            try
            {
                activity?.SetTag("Prefab", prefabVM.DisplayName);

                PrefabViewModel prefabToDelete = this.Prefabs.FirstOrDefault(c => c.PrefabId.Equals(prefabVM.PrefabId));
                if (prefabToDelete is not null)
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this test", "Confirm Delete", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        await this.prefabManager.DeletePrefabAsync(prefabVM.Prefab);   
                        this.Prefabs.Remove(prefabToDelete);
                        logger.Information("Deleted Prefab : '{0}'", prefabVM.DisplayName);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while trying to delete Test Case : '{0}'", prefabVM?.DisplayName);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }
    }

    /// <summary>
    /// Open a Prefab to be edited on automation editor
    /// </summary>
    /// <param name="prefabVM"></param>
    /// <returns></returns>
    async Task OpenPrefabAsync(PrefabViewModel prefabVM)
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(OpenPrefabAsync), ActivityKind.Internal))
        {
            Guard.Argument(prefabVM, nameof(prefabVM)).NotNull();
            activity?.SetTag("Prefab", prefabVM.DisplayName);

            await this.RefreshPrefabAsync(prefabVM);
            await this.prefabManager.DownloadPrefabDataAsync(prefabVM.Prefab);

            var prefabFiles = this.fileSystem.GetPrefabFiles(prefabVM.Prefab);
            prefabVM.WorkflowEntity = this.projectManager.Load<PrefabEntity>(prefabFiles.ProcessFile);

            if (await TryOpenForEdit())
            {
                SetupScriptEditor();
                prefabVM.IsOpenForEdit = true;
                NotifyOfPropertyChange(nameof(CanSaveAll));
            }

            async Task<bool> TryOpenForEdit()
            {
                var workflowEntity = prefabVM.WorkflowEntity;
                EntityManager prefabEntityManager = new EntityManager(this.entityManager);
                prefabEntityManager.SetIdentifier($"Prefab - {prefabVM.DisplayName}");
                workflowEntity.EntityManager = prefabEntityManager;               

                this.componentViewBuilder.OpenPrefab(prefabVM.Prefab);

                // Argumet is the supported shape instance so that there are no additional variables visible from test case data source type at design time
                prefabEntityManager.Arguments = Activator.CreateInstance(this.entityManager.Arguments.GetType().Assembly.FullName, prefabVM.SupportedShape).Unwrap();

                var scriptEngine = prefabEntityManager.GetScriptEngine();
                await scriptEngine.ExecuteFileAsync(prefabVM.ScriptFile);

                return true;

            }

            void SetupScriptEditor()
            {
                var prefabEntityManager = prefabVM.WorkflowEntity.EntityManager;
                IScriptEditorFactory editorFactory = prefabEntityManager.GetServiceOfType<IScriptEditorFactory>();

                //Add the test script project and script file to script editor
                editorFactory.AddProject(prefabVM.PrefabId, new string[] { }, prefabEntityManager.Arguments.GetType());
                string scriptFileContent = this.fileSystem.ReadAllText(prefabFiles.ScriptFile);
                editorFactory.AddDocument(prefabFiles.ScriptFile, prefabVM.PrefabId, scriptFileContent);
            }
        }
    }

    /// <summary>
    /// Close a Prefab which was previously opened to be edited on automation editor
    /// </summary>
    /// <param name="prefabVM"></param>
    /// <param name="autoSave"></param>
    /// <returns></returns>
    public async Task ClosePrefabAsync(PrefabViewModel prefabVM, bool autoSave)
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(ClosePrefabAsync), ActivityKind.Internal))
        {
            Guard.Argument(prefabVM, nameof(prefabVM)).NotNull();
            activity?.SetTag("Prefab", prefabVM.DisplayName);
            if (prefabVM.IsOpenForEdit)
            {
                if (autoSave)
                {
                    await SavePrefabDataAsync(prefabVM);
                }
            }
            NotifyOfPropertyChange(nameof(CanSaveAll));

            CleanUpScriptEditor();
            RemoveFromEditor();

            prefabVM.WorkflowEntity = null;
            prefabVM.IsOpenForEdit = false;

            logger.Information("Prefab : '{0}' was closed", prefabVM.DisplayName);

            void RemoveFromEditor()
            {
                var workflowEntity = prefabVM.WorkflowEntity;
                workflowEntity.EntityManager.Dispose();
                this.componentViewBuilder.ClosePrefab(prefabVM.Prefab);
            }

            void CleanUpScriptEditor()
            {
                //Remove the script editor project that was added while opening the test case
                var entityManager = prefabVM.WorkflowEntity.EntityManager;
                var scriptEditorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
                scriptEditorFactory.RemoveProject(prefabVM.PrefabId);
                prefabVM.WorkflowEntity.DisposeEditors();
            }
        }
    }

    /// <summary>
    /// Edit the initialization script for the Prefab
    /// </summary>
    /// <param name="prefabVM"></param>
    /// <returns></returns>
    public async Task EditPrefabScriptAsync(PrefabViewModel prefabVM)
    {
        try
        {
            Guard.Argument(prefabVM, nameof(prefabVM)).NotNull();
            if (prefabVM.IsOpenForEdit)
            {
                var entityManager = prefabVM.WorkflowEntity.EntityManager;
                var scriptEditorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
                using (IScriptEditorScreen scriptEditorScreen = scriptEditorFactory.CreateScriptEditorScreen())
                {
                    scriptEditorScreen.OpenDocument(prefabVM.ScriptFile, prefabVM.PrefabId, string.Empty);
                    var result = await this.windowManager.ShowDialogAsync(scriptEditorScreen);
                    if (result.HasValue && result.Value)
                    {
                        logger.Information("Script file for Prefab : '{0}' was edited.", prefabVM.ScriptFile);
                        var scriptEngine = entityManager.GetScriptEngine();
                        scriptEngine.ClearState();                       
                        await scriptEngine.ExecuteFileAsync(prefabVM.ScriptFile);
                    }
                }

            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error while trying to edit script file for Test Case : '{0}'", prefabVM?.DisplayName);
            await notificationManager.ShowErrorNotificationAsync(ex);
        }

    }

    /// <summary>
    /// Retrieve the latest version of test case and update the underlying model on view model to use it
    /// </summary>
    /// <param name="testCaseVM"></param>
    /// <returns></returns>
    async Task RefreshPrefabAsync(PrefabViewModel prefabVM)
    {
        var latestVersion = await this.prefabManager.GetPrefabAsync(prefabVM.PrefabId);
        if (latestVersion != null)
        {
            prefabVM.WithPrefab(latestVersion);
        }
    }

    /// <summary>
    /// Save data files belonging to the TestFixture
    /// </summary>
    /// <param name="fixtureVM"></param>
    /// <returns></returns>
    public async Task SavePrefabDataAsync(PrefabViewModel prefabVM)
    {
        if (prefabVM.IsDirty || prefabVM.IsOpenForEdit)
        {
            await this.prefabManager.UpdatePrefabAsync(prefabVM.Prefab);
            prefabVM.IsDirty = false;
        }
        await this.prefabManager.SavePrefabDataAsync(prefabVM.Prefab);
    }
  
    #endregion Test Case

    #region save all

    public bool CanSaveAll
    {
        get => this.Prefabs.Any(a => a.IsOpenForEdit);
    }

    /// <summary>
    /// Save all open test fixtures and test cases locally.     
    /// </summary>
    public async Task SaveAll()
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(SaveAll), ActivityKind.Internal))
        {
            foreach (var prefab in this.Prefabs)
            {
                try
                {
                    if (prefab.IsOpenForEdit)
                    {
                        await SavePrefabDataAsync(prefab);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error while trying to save prefab : {0}.", prefab.DisplayName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }
        }
    }

    #endregion save all

    #region life cycle

    /// <summary>
    /// Download all the Prefabs 
    /// </summary>
    private async Task DownloadPrefabsAsync()
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(DownloadPrefabsAsync), ActivityKind.Internal))
        {
            await this.prefabManager.DownloadAllPrefabsAsync();
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
            this.Prefabs.Clear();
            this.TestRunner = null;
        }
        return base.OnDeactivateAsync(close, cancellationToken);

    }

    public async Task HandleAsync(OpenPrefabRequestNotification message, CancellationToken cancellationToken)
    {
        try
        {
            Guard.Argument(message.PrefabId, nameof(OpenPrefabRequestNotification.PrefabId)).NotNull().NotEmpty();
            var targetPrefab = this.Prefabs.FirstOrDefault(t => t.PrefabId.Equals(message.PrefabId)) ??
                    throw new ArgumentException($"Prefab with Id : '{message.PrefabId}' was not found.");
            if (targetPrefab is not null)
            {
                if (targetPrefab.IsOpenForEdit)
                {
                    await notificationManager.ShowWarningNotificationAsync($"{targetPrefab.DisplayName} is already open for edit.");
                    logger.Information("Prefab : '{0}' is already open for edit", targetPrefab.DisplayName);
                    return;
                }
                message.PrefabHost.IsInDesignMode = true;
                await OpenPrefabAsync(targetPrefab);
            }
        }
        catch (Exception ex)
        {
            message.PrefabHost.IsInDesignMode = false;
            logger.Error(ex, "Error while trying to open prefab with Id : '{0}'", message.PrefabId);
            await notificationManager.ShowErrorNotificationAsync(ex);
        }       
    }

    public async Task HandleAsync(ClosePrefabRequestNotification message, CancellationToken cancellationToken)
    {
        try
        {
            Guard.Argument(message.PrefabId, nameof(message.PrefabId)).NotNull().NotEmpty();
            foreach (var prefab in this.Prefabs)
            {
                if (prefab.PrefabId.Equals(message.PrefabId))
                {
                    await ClosePrefabAsync(prefab, autoSave: message.SaveChanges);
                    break;
                }
            }
            message.PrefabHost.IsInDesignMode = false;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error while trying to close test case with Id : '{0}'", message.PrefabId);
            await notificationManager.ShowErrorNotificationAsync(ex);
        }       
    }

    public async Task HandleAsync(SavePrefabRequestNotification message, CancellationToken cancellationToken)
    {
        try
        {
            Guard.Argument(message.PrefabId, nameof(SavePrefabRequestNotification.PrefabId)).NotNull().NotEmpty();
            foreach (var prefab in this.Prefabs)
            {
                if (prefab.PrefabId.Equals(message.PrefabId))
                {
                    await SavePrefabDataAsync(prefab);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error while trying to save prefab with Id : '{0}'", message.PrefabId);
            await notificationManager.ShowErrorNotificationAsync(ex);
        }
    }

    #endregion
}