using Caliburn.Micro;
using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Contracts;
using Pixel.Automation.AppExplorer.ViewModels.Control;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefab;

/// <summary>
/// PrefabExplorer allows creating prefabs which are reusable components for a given application.
/// </summary>
public class PrefabExplorerViewModel : Screen, IApplicationAware, IHandle<OpenPrefabRequestNotification>,
    IHandle<ClosePrefabRequestNotification>, IHandle<SavePrefabRequestNotification>
{
    private readonly ILogger logger = Log.ForContext<PrefabExplorerViewModel>();
    private readonly IWindowManager windowManager;
    private readonly INotificationManager notificationManager;
    private readonly IEventAggregator eventAggregator;    
    private readonly IVersionManagerFactory versionManagerFactory;    
    private readonly IApplicationDataManager applicationDataManager;
    private readonly IPrefabDataManager prefabDataManager;   

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

    public BindableCollection<PrefabProjectViewModel> Prefabs { get; set; } = new ();
  
    public PrefabProjectViewModel SelectedPrefab { get; set; }

    //public PrefabDragHandler PrefabDragHandler { get; private set; } = new ();

    public PrefabExplorerViewModel(IEventAggregator eventAggregator, IWindowManager windowManager, 
        INotificationManager notificationManager, IVersionManagerFactory versionManagerFactory, IApplicationDataManager applicationDataManager, 
        IPrefabDataManager prefabDataManager)
    {
        this.DisplayName = "Prefab Explorer";
        this.eventAggregator = Guard.Argument(eventAggregator, nameof(eventAggregator)).NotNull().Value;
        this.eventAggregator.SubscribeOnPublishedThread(this);           
        this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;
        this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;
        this.versionManagerFactory = Guard.Argument(versionManagerFactory, nameof(versionManagerFactory)).NotNull().Value;          
        this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
        this.prefabDataManager = Guard.Argument(prefabDataManager, nameof(prefabDataManager)).NotNull().Value;
        CreateCollectionView();
    }

    /// <inheritdoc/>
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
                    var prefabsForSelectedScreen = LoadPrefabs(this.ActiveApplication, selectedScreen);                       
                    this.Prefabs.AddRange(prefabsForSelectedScreen);                       
                }
                else
                {
                    foreach (var screen in this.activeApplication.Screens)
                    {
                        var prefabsForScreen = LoadPrefabs(this.ActiveApplication, screen);
                        this.Prefabs.AddRange(prefabsForScreen);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to load prefabs for screen : '{0}'", selectedScreen?.ScreenName ?? "");
                _ = notificationManager.ShowErrorNotificationAsync(ex);
            }
        }
       
    }      

    private List<PrefabProjectViewModel> LoadPrefabs(ApplicationDescriptionViewModel applicationDescriptionViewModel, ApplicationScreen applicationScreen)
    {           
        List<PrefabProjectViewModel> prefabsList = new ();
        if (applicationDescriptionViewModel.ContainsScreen(applicationScreen.ScreenName))
        {
            var prefabIdentifiers = applicationDescriptionViewModel[applicationScreen.ScreenName].AvailablePrefabs;
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
                var prefabs = this.prefabDataManager.GetPrefabsForScreen(applicationDescriptionViewModel.Model, applicationScreen.ScreenName).ToList();
                foreach (var prefab in prefabs)
                {
                    if(prefab.IsDeleted)
                    {
                        continue;
                    }
                    var prefabProjectViewModel = new PrefabProjectViewModel(prefab);
                    applicationDescriptionViewModel.AddPrefab(prefabProjectViewModel, applicationScreen.ScreenName);
                    prefabsList.Add(prefabProjectViewModel);

                    if (this.openForEdit.ContainsKey(prefab.ProjectId))
                    {
                        prefabProjectViewModel.IsOpenInEditor = true;
                    }
                }
            }

        }
        return prefabsList;
    }


    /// Move a prefab from one screen to another
    /// </summary>
    /// <param name="controlDescription"></param>
    /// <returns></returns>
    public async Task MoveToScreen(PrefabProjectViewModel prefabProject)
    {
        Guard.Argument(prefabProject, nameof(prefabProject)).NotNull();

        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(MoveToScreen), ActivityKind.Internal))
        {
            try
            {
                activity?.SetTag("PrefabName", prefabProject.PrefabName);
                var moveToScreenViewModel = new MoveToScreenViewModel(prefabProject.PrefabName, this.ScreenCollection.Screens.Select(s => s.ScreenName), this.ScreenCollection.SelectedScreen.ScreenName);
                var result = await windowManager.ShowDialogAsync(moveToScreenViewModel);
                if (result.GetValueOrDefault())
                {
                    await this.prefabDataManager.MovePrefabToScreen(prefabProject.PrefabProject, this.ActiveApplication[moveToScreenViewModel.SelectedScreen].ScreenId);
                    this.ActiveApplication.MovePrefabToScreen(prefabProject, this.ScreenCollection.SelectedScreen.ScreenName, moveToScreenViewModel.SelectedScreen);                        
                    applicationDataManager.SaveApplicationToDisk(this.ActiveApplication.Model);
                    this.Prefabs.Remove(prefabProject);
                    logger.Information("Moved prefab : {0} from screen {1} to {2} for application {3}", prefabProject.PrefabName, this.ScreenCollection.SelectedScreen, moveToScreenViewModel.SelectedScreen, this.ActiveApplication.ApplicationName);
                    await notificationManager.ShowSuccessNotificationAsync($"Prefab : '{prefabProject.PrefabName}' was moved to screen : '{moveToScreenViewModel.SelectedScreen}'");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while moving prefab : '{0}' to another screen", prefabProject?.PrefabName);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }            
    }

    public async Task AddPrefab(ApplicationDescriptionViewModel forApplication)
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(AddPrefab), ActivityKind.Internal))
        {
            try
            {
                var newPrefabViewModel = new NewPrefabViewModel(forApplication, prefabDataManager);
                var result = await windowManager.ShowDialogAsync(newPrefabViewModel);
                if (result.GetValueOrDefault())
                {
                    var prefabViewModel = new PrefabProjectViewModel(newPrefabViewModel.NewProject);
                    this.Prefabs.Add(prefabViewModel);
                    this.ActiveApplication.AddPrefab(prefabViewModel, this.ScreenCollection.SelectedScreen.ScreenName);
                    this.applicationDataManager.SaveApplicationToDisk(this.ActiveApplication.Model);
                    //all the required files will be created when prefab is opened for the first time. We immediately do a save to make sure
                    //files are added to the database if working online
                    //await EditPrefab(prefabViewModel, prefabViewModel.PrefabProject.LatestActiveVersion); 
                    await this.prefabDataManager.SavePrefabDataAsync(prefabViewModel.PrefabProject, prefabViewModel.PrefabProject.LatestActiveVersion);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while creating a new prefab for application : '{0}'", forApplication.ApplicationName);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }             
        }
    }   

    async Task<IPrefabEditor> EditPrefab(PrefabProject prefabToEdit, VersionInfo version, PrefabHostEntity prefabHost)
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(EditPrefab), ActivityKind.Internal))
        {
            activity?.SetTag("PrefabName", prefabToEdit.Name);
            activity?.SetTag("PrefabVersion", version);

            var editorFactory = IoC.Get<IEditorFactory>();
            var prefabEditor = editorFactory.CreatePrefabEditor();
            var workflowEntity = await prefabEditor.LoadAsync(prefabToEdit, version);

            if (await TryOpenForEdit())
            {
                NotifyOfPropertyChange(nameof(CanSaveAll));
            }

            return prefabEditor;

            async Task<bool> TryOpenForEdit()
            {
                var componentViewBuilder = prefabHost.EntityManager.GetServiceOfType<IComponentViewBuilder>();
                componentViewBuilder.OpenPrefab(prefabToEdit, workflowEntity);

                //TypeExtensions.CopyProperties(prefabHost.EntityManager.Arguments, workflowEntity.EntityManager.Arguments);

                return true;
            }
        }
    }

    /// <summary>
    /// Close a Prefab which was previously opened to be edited on automation editor
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="autoSave"></param>
    /// <returns></returns>
    async Task ClosePrefabAsync(IPrefabEditor editor, PrefabHostEntity prefabHost, bool autoSave)
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(ClosePrefabAsync), ActivityKind.Internal))
        {
            Guard.Argument(editor, nameof(editor)).NotNull();
            activity?.SetTag("Prefab", editor.PrefabProject.Name);
            activity?.SetTag("AutoSave", autoSave);

            RemoveFromEditor();

            if (autoSave)
            {
                await editor.SaveAsync();
            }

            editor.Dispose();            
            NotifyOfPropertyChange(nameof(CanSaveAll));          

            logger.Information("Prefab : '{0}' was closed", editor.PrefabProject.Name);

            void RemoveFromEditor()
            {
                var componentViewBuilder = prefabHost.EntityManager.GetServiceOfType<IComponentViewBuilder>();
                componentViewBuilder.ClosePrefab(editor.PrefabProject, editor.RootEntity);
            }
        }
    }


    /// <summary>
    /// Open the version manager screen that can be used to deploy current version of Prefab.
    /// Only deployed versions can be used in another automatio project.
    /// </summary>
    /// <param name="targetPrefab"></param>
    /// <returns></returns>
    public async Task ManagePrefab(PrefabProjectViewModel targetPrefab)
    {
        Guard.Argument(targetPrefab, nameof(targetPrefab)).NotNull();
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(ManagePrefab), ActivityKind.Internal))
        {
            try
            {
                activity?.SetTag("PrefabName", targetPrefab.PrefabName);
                var deployPrefabViewModel = versionManagerFactory.CreatePrefabVersionManager(targetPrefab.PrefabProject);
                await windowManager.ShowDialogAsync(deployPrefabViewModel);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to manage prefab");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }           
    }

    /// <summary>
    /// Delete the prefab
    /// </summary>
    /// <param name="prefabToDelete"></param>
    public async Task DeletePrefabAsync(PrefabProjectViewModel prefabToDelete)
    {
        Guard.Argument(prefabToDelete, nameof(prefabToDelete)).NotNull();
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(DeletePrefabAsync), ActivityKind.Internal))
        {
            try
            {
                activity?.SetTag("PrefabName", prefabToDelete.PrefabName);
                await this.prefabDataManager.DeletePrefbAsync(prefabToDelete.PrefabProject);
                this.ActiveApplication.DeletePrefab(prefabToDelete, this.ActiveApplication.ScreenCollection.SelectedScreen.ScreenName);
                this.Prefabs.Remove(prefabToDelete);
                this.applicationDataManager.SaveApplicationToDisk(this.ActiveApplication.Model);
                await notificationManager.ShowSuccessNotificationAsync($"Prefab : '{prefabToDelete.PrefabName}' was deleted");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to delete prefab : {0}", prefabToDelete.PrefabName);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }           
    }


    /// <summary>
    /// Broadcast a FilterTestMessage which is processed by Test explorer view to filter and show only those test cases
    /// which uses this prefab
    /// </summary>
    /// <param name="targetPrefab"></param>
    /// <returns></returns>
    public async Task ShowUsage(PrefabProjectViewModel targetPrefab)
    {
        await this.eventAggregator.PublishOnUIThreadAsync(new TestFilterNotification("prefab", targetPrefab.PrefabId));
    }

    /// <summary>
    /// Manage the controls version used by Prefab
    /// </summary>     
    public async Task ManageControlReferencesAsync(PrefabProjectViewModel targetPrefab)
    {
        if (openForEdit.ContainsKey(targetPrefab.PrefabId))
        {
            var prefabEditor = openForEdit[targetPrefab.PrefabId];
            await prefabEditor.ManageControlReferencesAsync();
        }
    }

    /// <summary>
    /// Manage the assembly references used by Prefab
    /// </summary>    
    public async Task ManageAssemblyReferencesAsync(PrefabProjectViewModel targetPrefab)
    {
        if (openForEdit.ContainsKey(targetPrefab.PrefabId))
        {
            var prefabEditor = openForEdit[targetPrefab.PrefabId];
            await prefabEditor.ManageAssemblyReferencesAsync();
        }
    }

    /// <summary>
    /// Open the code editor to edit Prefab data model
    /// </summary> 
    public async Task EditDataModelAsync(PrefabProjectViewModel targetPrefab)
    {
        if (openForEdit.ContainsKey(targetPrefab.PrefabId))
        {
            var prefabEditor = openForEdit[targetPrefab.PrefabId];
            await prefabEditor.EditDataModelAsync();
        }
    }

    /// <summary>
    /// Open the script editor to edit Prefab's initialization script
    /// </summary>   
    public async Task EditScriptAsync(PrefabProjectViewModel targetPrefab)
    {
        if (openForEdit.ContainsKey(targetPrefab.PrefabId))
        {
            var prefabEditor = openForEdit[targetPrefab.PrefabId];
            await prefabEditor.EditScriptAsync();
        }
    }

    #region Filter 


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

            var view = CollectionViewSource.GetDefaultView(Prefabs);
            view.Refresh();
            NotifyOfPropertyChange(() => Prefabs);

        }
    }

    private void CreateCollectionView()
    {
        var groupedItems = CollectionViewSource.GetDefaultView(Prefabs);
        groupedItems.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PrefabProjectViewModel.GroupName)));
        groupedItems.SortDescriptions.Add(new SortDescription(nameof(PrefabProjectViewModel.GroupName), ListSortDirection.Ascending));
        groupedItems.SortDescriptions.Add(new SortDescription(nameof(PrefabProjectViewModel.PrefabName), ListSortDirection.Ascending));
        groupedItems.Filter = new Predicate<object>((a) =>
        {
            if (a is PrefabProjectViewModel prefabProject)
            {
                return prefabProject.PrefabName.Contains(this.filterText, StringComparison.OrdinalIgnoreCase) ||
                        prefabProject.GroupName.Contains(this.filterText, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        });
    }
  
    #endregion Filter      

    #region save all

    public bool CanSaveAll
    {
        get => this.Prefabs.Any(a => a.IsOpenInEditor);
    }

    /// <summary>
    /// Save all open prefabs.     
    /// </summary>
    public async Task SaveAll()
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(SaveAll), ActivityKind.Internal))
        {
            foreach (var prefab in this.openForEdit.Values)
            {
                try
                {
                    await prefab.SaveAsync();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error while trying to save prefab : {0}.", prefab.PrefabProject.Name);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }
        }
    }

    #endregion save all


    #region Notifications

    private readonly Dictionary<string, IPrefabEditor> openForEdit = new();

    public async Task HandleAsync(OpenPrefabRequestNotification message, CancellationToken cancellationToken)
    {
        try
        {
            Guard.Argument(message.PrefabId, nameof(OpenPrefabRequestNotification.PrefabId)).NotNull().NotEmpty();

            var hostEntityManager = message.PrefabHost.EntityManager;
            var referenceProvider = hostEntityManager.GetReferenceProvider();
            var versionInUse = referenceProvider.GetPrefabVersion(message.PrefabHost.ApplicationId, message.PrefabHost.PrefabId);
            var targetPrefab = this.prefabDataManager.GetAllPrefabs(message.PrefabHost.ApplicationId).Where(p => p.ApplicationId == message.PrefabHost.ApplicationId && p.ProjectId == message.PrefabId).First();
            if (targetPrefab is not null)
            {
                if (openForEdit.ContainsKey(targetPrefab.ProjectId))
                {
                    await notificationManager.ShowWarningNotificationAsync($"{targetPrefab.Name} is already open for edit.");
                    logger.Information("Prefab : '{0}' is already open for edit", targetPrefab.Name);
                    return;
                }
                message.PrefabHost.IsInDesignMode = true;

                var openPrefabInstance = await EditPrefab(targetPrefab, versionInUse, message.PrefabHost);
                openForEdit[targetPrefab.ProjectId] = openPrefabInstance;

                MarkOpenForEdit(message);
            }
        }
        catch (Exception ex)
        {
            message.PrefabHost.IsInDesignMode = false;
            logger.Error(ex, "Error while trying to open prefab with Id : '{0}'", message.PrefabId);
            await notificationManager.ShowErrorNotificationAsync(ex);
        }

        void MarkOpenForEdit(OpenPrefabRequestNotification message)
        {
            var prefabVM = this.Prefabs.FirstOrDefault(p => p.PrefabId.Equals(message.PrefabId));
            if (prefabVM is not null)
            {
                prefabVM.IsOpenInEditor = true;
            }
        }
    }

    public async Task HandleAsync(SavePrefabRequestNotification message, CancellationToken cancellationToken)
    {
        try
        {
            Guard.Argument(message.PrefabId, nameof(SavePrefabRequestNotification.PrefabId)).NotNull().NotEmpty();

            if (this.openForEdit.ContainsKey(message.PrefabId))
            {
                var editor = openForEdit[message.PrefabId];
                await editor.SaveAsync();
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error while trying to save prefab with Id : '{0}'", message.PrefabId);
            await notificationManager.ShowErrorNotificationAsync(ex);
        }
    }

    public async Task HandleAsync(ClosePrefabRequestNotification message, CancellationToken cancellationToken)
    {
        try
        {
            Guard.Argument(message.PrefabId, nameof(message.PrefabId)).NotNull().NotEmpty();
            if (this.openForEdit.ContainsKey(message.PrefabId))
            {
                var prefabEditor = openForEdit[message.PrefabId];
                await ClosePrefabAsync(prefabEditor, message.PrefabHost, message.SaveChanges);             
            }
            openForEdit.Remove(message.PrefabId);
            message.PrefabHost.IsInDesignMode = false;
            MarkClosedForEdit(message);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error while trying to close test case with Id : '{0}'", message.PrefabId);
            await notificationManager.ShowErrorNotificationAsync(ex);
        }

        void MarkClosedForEdit(ClosePrefabRequestNotification message)
        {
            var prefabVM = this.Prefabs.FirstOrDefault(p => p.PrefabId.Equals(message.PrefabId));
            if (prefabVM is not null)
            {
                prefabVM.IsOpenInEditor = false;
            }
        }      
    }

    #endregion
}
