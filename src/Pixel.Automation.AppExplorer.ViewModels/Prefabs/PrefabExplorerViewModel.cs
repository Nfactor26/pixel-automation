using Caliburn.Micro;
using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Contracts;
using Pixel.Automation.AppExplorer.ViewModels.Control;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Notifications.Notfications;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefabs;


public class PrefabExplorerViewModel : Screen, IApplicationAware,
    IHandle<OpenPrefabRequestNotification>, IHandle<ClosePrefabRequestNotification>, IHandle<SavePrefabRequestNotification>
{
    #region Data members

    private readonly ILogger logger = Log.ForContext<PrefabExplorerViewModel>();
    
    private readonly IApplicationDataManager applicationDataManager;   
    private readonly IEventAggregator eventAggregator;
    private readonly IWindowManager windowManager;
    private readonly INotificationManager notificationManager;   
    private readonly IPrefabDataManager prefabManager;     
    
    private ApplicationDescriptionViewModel activeApplication;

    public ApplicationDescriptionViewModel ActiveApplication
    {
        get => this.activeApplication;
        set
        {
            this.activeApplication = value;
            NotifyOfPropertyChange();
        }
    }

    public ApplicationScreenCollection ScreenCollection { get; private set; }

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
            this.eventAggregator.PublishOnUIThreadAsync(new PropertyGridObjectEventArgs(value, true));
        }
    }   

    #endregion Data members

    #region Constructor

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="eventAggregator"></param>
    /// <param name="projectManager"></param>   
    /// <param name="entityManager"></param>
    /// <param name="windowManager"></param>
    /// <param name="applicationSettings"></param>
    public PrefabExplorerViewModel(IEventAggregator eventAggregator, IPrefabDataManager prefabManager, IApplicationDataManager applicationDataManager,
        IWindowManager windowManager, INotificationManager notificationManager)
    {
        this.DisplayName = "Prefab Explorer";
        this.prefabManager = Guard.Argument(prefabManager, nameof(prefabManager)).NotNull().Value;
        this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
        this.eventAggregator = Guard.Argument(eventAggregator, nameof(eventAggregator)).NotNull().Value;
        this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;
        this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;            
        this.eventAggregator.SubscribeOnPublishedThread(this);        

        CreateCollectionView();
    }

    #endregion

    #region Filter

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


    /// <summary>
    /// Setup the collection view with grouping and sorting
    /// </summary>
    private void CreateCollectionView()
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

    #endregion

    #region Application Aware

    /// <summary>
    /// Set the application for which control collection should be loaded and displayed
    /// </summary>
    /// <param name="applicationDescriptionViewModel"></param>
    public void SetActiveApplication(ApplicationDescriptionViewModel applicationDescriptionViewModel)
    {
        if (this.ScreenCollection != null)
        {
            this.ScreenCollection.ScreenChanged -= OnScreenChanged;
        }
        this.ActiveApplication = applicationDescriptionViewModel;
        this.ScreenCollection = applicationDescriptionViewModel?.ScreenCollection;
        this.Prefabs.Clear();
        if (this.ScreenCollection != null)
        {
            this.ScreenCollection.ScreenChanged += OnScreenChanged;
            OnScreenChanged(this, this.ScreenCollection.SelectedScreen);
        }
        NotifyOfPropertyChange(nameof(this.ScreenCollection));
    }

    private void OnScreenChanged(object sender, ApplicationScreen selectedScreen)
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(OnScreenChanged), ActivityKind.Internal))
        {
            try
            {
                this.Prefabs.Clear();
                activity?.SetTag("SelectedScreen", selectedScreen?.ScreenName ?? string.Empty);
                if (selectedScreen != null)
                {
                    var prefabsForSelectedScreen = LoadPrefabDetails(this.ActiveApplication, selectedScreen);
                    this.Prefabs.AddRange(prefabsForSelectedScreen);
                }
                else
                {
                    foreach (var screen in this.activeApplication.Screens)
                    {
                        var prefabsForScreen = LoadPrefabDetails(this.ActiveApplication, screen);
                        this.Prefabs.AddRange(prefabsForScreen);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occured while loading controls for screen : '{0}'", selectedScreen);
                _ = notificationManager.ShowErrorNotificationAsync(ex);
            }
        }
    }

    private IEnumerable<PrefabViewModel> LoadPrefabDetails(ApplicationDescriptionViewModel applicationDescriptionViewModel, ApplicationScreen selectedScreen)
    {
        List<PrefabViewModel> prefabsList = new();
        if (applicationDescriptionViewModel.ContainsScreen(selectedScreen.ScreenName))
        {
            var prefabIdentifiers = selectedScreen.AvailablePrefabs;
            if (prefabIdentifiers.Any() && applicationDescriptionViewModel.PrefabsCollection.Any(a => a.PrefabId.Equals(prefabIdentifiers.First())))
            {
                foreach (var prefabId in prefabIdentifiers)
                {
                    var prefabs = applicationDescriptionViewModel.PrefabsCollection.Where(a => a.PrefabId.Equals(prefabId));
                    if (prefabs.Any())
                    {
                        prefabsList.AddRange(prefabs);
                    }
                }
            }
            else
            {
                var prefabs = this.prefabManager.GetPrefabsForScreen(applicationDescriptionViewModel.Model, selectedScreen.ScreenName).ToList();
                foreach (var prefab in prefabs)
                {
                    if (prefab.IsDeleted)
                    {
                        continue;
                    }
                    var prefabViewModel = new PrefabViewModel(prefab);
                    applicationDescriptionViewModel.AddPrefab(prefabViewModel, selectedScreen.ScreenName);
                    prefabsList.Add(prefabViewModel);
                }
            }

        }
        return prefabsList;
    }

    /// <summary>
    /// Move a Prefab from one screen to another
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public async Task MoveToScreen(PrefabViewModel prefab)
    {
        Guard.Argument(prefab, nameof(prefab)).NotNull();
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(MoveToScreen), ActivityKind.Internal))
        {
            try
            {
                activity?.SetTag("PrefabName", prefab.DisplayName);
                var moveToScreenViewModel = new MoveToScreenViewModel(prefab.DisplayName, this.ScreenCollection.Screens.Select(s => s.ScreenName), this.ScreenCollection.SelectedScreen.ScreenName);
                var result = await windowManager.ShowDialogAsync(moveToScreenViewModel);
                if (result.GetValueOrDefault())
                {
                    await this.prefabManager.MovePrefabToScreen(prefab.Prefab, this.ActiveApplication[moveToScreenViewModel.SelectedScreen].ScreenId);
                    this.ActiveApplication.MovePrefabToScreen(prefab, this.ScreenCollection.SelectedScreen.ScreenName, moveToScreenViewModel.SelectedScreen);
                    this.Prefabs.Remove(prefab);
                    this.applicationDataManager.SaveApplicationToDisk(this.ActiveApplication.Model);
                    logger.Information("Moved Prefab : {0} from screen {1} to {2} for application {3}", prefab.DisplayName, this.ScreenCollection.SelectedScreen, moveToScreenViewModel.SelectedScreen, this.ActiveApplication.ApplicationName);
                    await notificationManager.ShowSuccessNotificationAsync($"Prefab : '{prefab.DisplayName}' was moved to screen : '{moveToScreenViewModel.SelectedScreen}'");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while moving control : '{0}' to another screen", prefab?.DisplayName);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }
    }

    #endregion 

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
                    ApplicationId = this.ActiveApplication.ApplicationId,
                    Version = new Version(1,0),
                    WorkFlowEntity = new PrefabEntity()                   
                };
               
                var prefabEditor = new EditPrefabViewModel(prefab, this.Prefabs.Select(s => s.DisplayName), windowManager);
                bool? result = await this.windowManager.ShowDialogAsync(prefabEditor);
                if (result.HasValue && result.Value)
                {
                    prefab.WorkFlowEntity.Name = prefab.DisplayName;
                    PrefabViewModel prefabVM = new PrefabViewModel(prefab);
                    this.ActiveApplication.AddPrefab(prefabVM, this.ScreenCollection.SelectedScreen.ScreenName);
                    this.applicationDataManager.SaveApplicationToDisk(this.ActiveApplication.Model);
                    await this.prefabManager.AddPrefabToScreenAsync(prefab, this.ActiveApplication.ScreenCollection.SelectedScreen.ScreenName); 
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
                var prefabEditor = new EditPrefabViewModel(prefabVM.Prefab, this.Prefabs.Except([prefabVM]).Select(s => s.DisplayName), windowManager);
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
    /// Create a copy of Prefab
    /// </summary>
    /// <param name="controlToRename"></param>
    public async Task CreateRevisionAsync(PrefabViewModel prefab)
    {
        Guard.Argument(prefab, nameof(prefab)).NotNull();
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(CreateRevisionAsync), ActivityKind.Internal))
        {
            try
            {
                activity?.SetTag("Prefab", prefab.DisplayName);
                
                var clone = await this.prefabManager.CreateRevision(prefab.Prefab, new Version(prefab.Version.Major + 1, 0));
                this.Prefabs.Remove(prefab); //Remove current version from Prefabs collection
                this.Prefabs.Add(new PrefabViewModel(clone));

                logger.Information("Created a new revision for Prefab : {0}", clone.DisplayName);
                await notificationManager.ShowSuccessNotificationAsync($"Version of Prefab : '{clone.DisplayName}' was incremented to : '{clone.Version}'");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while creating revision of Prefab : {0}", prefab?.DisplayName ?? "NULL");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }
    }

    /// <summary>
    /// Edit the initialization script for a Prefab
    /// </summary>
    /// <param name="fixtureVM"></param>
    /// <returns></returns>
    public async Task EditPrefabScriptAsync(PrefabViewModel prefabVM)
    {
        try
        {          
            if (prefabVM.IsOpenForEdit)
            {
                var prefabEntityManager = prefabVM.WorkflowEntity.EntityManager;
                IScriptEditorFactory scriptEditorFactory = prefabEntityManager.GetServiceOfType<IScriptEditorFactory>();
                using (IScriptEditorScreen scriptEditorScreen = scriptEditorFactory.CreateScriptEditorScreen())
                {
                    scriptEditorScreen.OpenDocument(prefabVM.ScriptFile, prefabVM.PrefabId, string.Empty);
                    var result = await this.windowManager.ShowDialogAsync(scriptEditorScreen);
                    if (result.HasValue && result.Value)
                    {
                        logger.Information("Script file : '{0}' was edited for Prefab : '{1}'.", prefabVM.ScriptFile, prefabVM.DisplayName);
                        var prefabScriptEngine = prefabEntityManager.GetScriptEngine();
                        prefabScriptEngine.ClearState();
                        await prefabScriptEngine.ExecuteFileAsync(prefabVM.ScriptFile);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error while trying to edit script file for Prefab : '{0}'", prefabVM?.DisplayName);
            await notificationManager.ShowErrorNotificationAsync(ex);
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
    async Task OpenPrefabAsync(PrefabViewModel prefabVM , PrefabHostEntity prefabHost)
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(OpenPrefabAsync), ActivityKind.Internal))
        {
            Guard.Argument(prefabVM, nameof(prefabVM)).NotNull();
            activity?.SetTag("Prefab", prefabVM.DisplayName);
           
            await this.prefabManager.DownloadPrefabDataAsync(prefabVM.Prefab);

            var prefabFiles = this.prefabManager.GetPrefabFiles(prefabVM.Prefab);
            prefabVM.WorkflowEntity = await this.prefabManager.GetPrefabEntity(prefabVM.Prefab);

            if (await TryOpenForEdit())
            {
                SetupScriptEditor();
                prefabVM.IsOpenForEdit = true;
                NotifyOfPropertyChange(nameof(CanSaveAll));
            }

            async Task<bool> TryOpenForEdit()
            {
                var workflowEntity = prefabVM.WorkflowEntity;
                EntityManager prefabEntityManager = new EntityManager(prefabHost.EntityManager);
                prefabEntityManager.SetIdentifier($"Prefab - {prefabVM.DisplayName}");
                workflowEntity.EntityManager = prefabEntityManager;
                 
                var componentViewBuilder = prefabHost.EntityManager.GetServiceOfType<IComponentViewBuilder>();
                componentViewBuilder.OpenPrefab(prefabVM.Prefab);

                // TODO : This should be a design time argument for prefab
                prefabEntityManager.Arguments = prefabHost.EntityManager.Arguments;

                //Copy the values from the arguments of testcase entity manager to prefab entity manager. This will allow to execute components of prefab at design time one at a time.
                //TypeExtensions.CopyProperties(workflowEntity.Parent.EntityManager.Arguments, prefabEntityManager.Arguments);

                var scriptEngine = prefabEntityManager.GetScriptEngine();
                await scriptEngine.ExecuteFileAsync(prefabFiles.ScriptFile);

                return true;

            }

            void SetupScriptEditor()
            {
                var prefabEntityManager = prefabVM.WorkflowEntity.EntityManager;
                IScriptEditorFactory editorFactory = prefabEntityManager.GetServiceOfType<IScriptEditorFactory>();

                //Add the test script project and script file to script editor
                editorFactory.AddProject(prefabVM.PrefabId, new string[] { }, prefabEntityManager.Arguments.GetType());
                string scriptFileContent = File.ReadAllText(prefabFiles.ScriptFile);
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
                    await SavePrefabAsync(prefabVM);
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

                var componentViewBuilder = workflowEntity.Parent.EntityManager.GetServiceOfType<IComponentViewBuilder>();              
                componentViewBuilder.ClosePrefab(prefabVM.Prefab);
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
    /// Retrieve the latest version of test case and update the underlying model on view model to use it
    /// </summary>
    /// <param name="testCaseVM"></param>
    /// <returns></returns>
    async Task RefreshPrefabAsync(PrefabViewModel prefabVM)
    {
        var latestVersion = await this.prefabManager.GetPrefabByIdAsync(prefabVM.ApplicationId, prefabVM.PrefabId, prefabVM.Version.ToString());
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
    public async Task SavePrefabAsync(PrefabViewModel prefabVM)
    {
        if (prefabVM.IsDirty)
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
                        await SavePrefabAsync(prefab);
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
        }
        return base.OnDeactivateAsync(close, cancellationToken);

    }

    private readonly IList<PrefabViewModel> openForEdit = new List<PrefabViewModel>();

    public async Task HandleAsync(OpenPrefabRequestNotification message, CancellationToken cancellationToken)
    {
        try
        {
            Guard.Argument(message.PrefabId, nameof(OpenPrefabRequestNotification.PrefabId)).NotNull().NotEmpty();
            var hostEntityManager = message.PrefabHost.EntityManager;
            var referenceManager = hostEntityManager.GetServiceOfType<IReferenceManager>();
            var versionInUse = referenceManager.GetPrefabReferences().GetPrefabReference(message.PrefabId).Version;
            var targetPrefab = await this.prefabManager.GetPrefabByIdAsync(message.PrefabHost.ApplicationId, message.PrefabId, versionInUse.ToString());
            if (targetPrefab is not null)
            {
                if (openForEdit.Any(p => p.PrefabId.Equals(targetPrefab.PrefabId)))
                {
                    await notificationManager.ShowWarningNotificationAsync($"{targetPrefab.DisplayName} is already open for edit.");
                    logger.Information("Prefab : '{0}' is already open for edit", targetPrefab.DisplayName);
                    return;
                }
                message.PrefabHost.IsInDesignMode = true;
                var prefabVM = new PrefabViewModel(targetPrefab);               
                await OpenPrefabAsync(prefabVM, message.PrefabHost);
                openForEdit.Add(prefabVM);
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
            foreach (var prefab in this.openForEdit)
            {
                if (prefab.PrefabId.Equals(message.PrefabId))
                {
                    await ClosePrefabAsync(prefab, autoSave: message.SaveChanges);
                    openForEdit.Remove(prefab);
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
                    await SavePrefabAsync(prefab);
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