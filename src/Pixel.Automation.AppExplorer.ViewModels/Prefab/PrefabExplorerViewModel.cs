using Caliburn.Micro;
using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Contracts;
using Pixel.Automation.AppExplorer.ViewModels.Control;
using Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Notifications.Notfications;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefab
{
    /// <summary>
    /// PrefabExplorer allows creating prefabs which are reusable components for a given application.
    /// </summary>
    public class PrefabExplorerViewModel : Screen, IApplicationAware, IHandle<EditorClosedNotification<PrefabProject>>, IHandle<OpenPrefabVersionForEditNotification>
    {
        private readonly ILogger logger = Log.ForContext<PrefabExplorerViewModel>();
        private readonly IWindowManager windowManager;
        private readonly INotificationManager notificationManager;
        private readonly IEventAggregator eventAggregator;    
        private readonly IVersionManagerFactory versionManagerFactory;
        private readonly IApplicationDataManager applicationDataManager;
        private readonly IPrefabDataManager prefabDataManager;
        private IPrefabBuilderFactory prefabBuilderFactory;


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

        public PrefabDragHandler PrefabDragHandler { get; private set; } = new ();

        public PrefabExplorerViewModel(IEventAggregator eventAggregator, IWindowManager windowManager, 
            INotificationManager notificationManager, IVersionManagerFactory versionManagerFactory, 
            IApplicationDataManager applicationDataManager, IPrefabDataManager prefabDataManager, IPrefabBuilderFactory prefabBuilderFactory)
        {
            this.DisplayName = "Prefab Explorer";
            this.eventAggregator = Guard.Argument(eventAggregator, nameof(eventAggregator)).NotNull().Value;
            this.eventAggregator.SubscribeOnPublishedThread(this);
            this.prefabBuilderFactory = Guard.Argument(prefabBuilderFactory, nameof(prefabBuilderFactory)).NotNull().Value;
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

        private void OnScreenChanged(object sender, string selectedScreen)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(OnScreenChanged), ActivityKind.Internal))
            {
                try
                {
                    activity?.SetTag("SelectedScreen", selectedScreen);
                    if (!string.IsNullOrEmpty(selectedScreen))
                    {
                        var prefabsForSelectedScreen = LoadPrefabs(this.ActiveApplication, selectedScreen);
                        this.Prefabs.Clear();
                        this.Prefabs.AddRange(prefabsForSelectedScreen);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to load prefabs for screen : '{0}'", selectedScreen);
                    _ = notificationManager.ShowErrorNotificationAsync(ex);
                }
            }
           
        }      

        private List<PrefabProjectViewModel> LoadPrefabs(ApplicationDescriptionViewModel applicationDescriptionViewModel, string screenName)
        {           
            List<PrefabProjectViewModel> prefabsList = new ();
            if (applicationDescriptionViewModel.AvailablePrefabs.ContainsKey(screenName))
            {
                var prefabIdentifiers = applicationDescriptionViewModel.AvailablePrefabs[screenName];
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
                    var prefabs = this.prefabDataManager.GetPrefabsForScreen(applicationDescriptionViewModel.Model, screenName).ToList();
                    foreach (var prefab in prefabs)
                    {
                        if(prefab.IsDeleted)
                        {
                            continue;
                        }
                        var prefabProjectViewModel = new PrefabProjectViewModel(prefab);
                        applicationDescriptionViewModel.AddPrefab(prefabProjectViewModel, screenName);
                        prefabsList.Add(prefabProjectViewModel);
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
                    var moveToScreenViewModel = new MoveToScreenViewModel(prefabProject.PrefabName, this.ScreenCollection.Screens, this.ScreenCollection.SelectedScreen);
                    var result = await windowManager.ShowDialogAsync(moveToScreenViewModel);
                    if (result.GetValueOrDefault())
                    {
                        this.ActiveApplication.MovePrefabToScreen(prefabProject, this.ScreenCollection.SelectedScreen, moveToScreenViewModel.SelectedScreen);
                        await this.applicationDataManager.AddOrUpdateApplicationAsync(this.ActiveApplication.Model);
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

        /// <summary>
        /// Show PrefabBuilder wizard screen that can be used to configure and create a new Prefab for the specified Entity.
        /// The entity is dragged and dropped from process designer to the PrefabExplorer to initiate the wizard.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task CreatePrefab(Entity entity)
        {
            Guard.Argument(entity, nameof(entity)).NotNull();
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(CreatePrefab), ActivityKind.Internal))
            {
                try
                {
                    activity?.SetTag("FromEntity", entity.Id);
                    var prefabBuilder = prefabBuilderFactory.CreatePrefabBuilder();
                    prefabBuilder.Initialize(this.ActiveApplication, entity);
                    var result = await windowManager.ShowDialogAsync(prefabBuilder);
                    if (result.HasValue && result.Value)
                    {
                        var createdPrefab = await prefabBuilder.SavePrefabAsync();
                        var prefabViewModel = new PrefabProjectViewModel(createdPrefab);
                        this.Prefabs.Add(prefabViewModel);
                        this.ActiveApplication.AddPrefab(prefabViewModel, this.ScreenCollection.SelectedScreen);
                        await this.applicationDataManager.AddOrUpdateApplicationAsync(this.ActiveApplication.Model);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to create a new prefab");
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }          
        }

        private readonly object locker = new();
        /// <summary>
        /// Open Prefab process for editing in process designer
        /// </summary>
        /// <param name="prefabToEdit"></param>
        public async Task EditPrefab(PrefabProjectViewModel prefabToEdit)
        {
            var versionPicker = new PrefabVersionPickerViewModel(prefabToEdit.PrefabProject);
            var result = await windowManager.ShowDialogAsync(versionPicker);
            var versionToEdit = versionPicker.SelectedVersion;
            if (result.HasValue && result.Value)
            {
                await EditPrefab(prefabToEdit, versionToEdit);
            }
        }

        private async Task EditPrefab(PrefabProjectViewModel prefabToEdit, VersionInfo prefabVersion)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(EditPrefab), ActivityKind.Internal))
            {
                try
                {
                    lock (locker)
                    {
                        if (prefabToEdit.IsOpenInEditor)
                        {
                            logger.Information($"Project {prefabToEdit.PrefabName} is already open.");
                            return;
                        }
                        prefabToEdit.IsOpenInEditor = true;
                    }
                    activity?.SetTag("PrefabName", prefabToEdit.PrefabName);
                    activity?.SetTag("PrefabVersion", prefabVersion);

                    var editorFactory = IoC.Get<IEditorFactory>();
                    var prefabEditor = editorFactory.CreatePrefabEditor();
                    await prefabEditor.DoLoad(prefabToEdit.PrefabProject, prefabVersion);
                    await this.eventAggregator.PublishOnUIThreadAsync(new ActivateScreenNotification() { ScreenToActivate = prefabEditor as IScreen });
                }
                catch (Exception ex)
                {
                    prefabToEdit.IsOpenInEditor = false;
                    logger.Error(ex, "There was an error while trying to open version : '{0}' of prefab : '{1}' for edit", prefabVersion, prefabToEdit.PrefabName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
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
                    this.Prefabs.Remove(prefabToDelete);
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
                return (a as PrefabProjectViewModel).PrefabName.ToLower().Contains(this.filterText.ToLower());
            });
        }

        /// <summary>
        /// Notificaton handler for <see cref="EditorClosedNotification"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task HandleAsync(EditorClosedNotification<PrefabProject> message, CancellationToken cancellationToken)
        {
            try
            {
                var project = this.Prefabs.FirstOrDefault(a => a.PrefabId.Equals(message.Project.ProjectId));
                if (project != null)
                {
                    project.IsOpenInEditor = false;                   
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occured while handling the notification for editor closed");
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Notification handler for <see cref="OpenPrefabVersionForEditNotification"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task HandleAsync(OpenPrefabVersionForEditNotification message, CancellationToken cancellationToken)
        {
            try
            {
                var prefabProject = this.Prefabs.FirstOrDefault(p => p.PrefabId.Equals(message.PrefabProject.ProjectId));
                if (prefabProject != null)
                {
                    await EditPrefab(prefabProject, message.VersionToOpen);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }

        #endregion Filter      
    }
}
