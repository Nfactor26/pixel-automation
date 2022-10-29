using Caliburn.Micro;
using Dawn;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Contracts;
using Pixel.Automation.AppExplorer.ViewModels.Control;
using Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Serilog;
using System.ComponentModel;
using System.Windows.Data;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefab
{
    /// <summary>
    /// PrefabExplorer allows creating prefabs which are reusable components for a given application.
    /// </summary>
    public class PrefabExplorerViewModel : Screen, IApplicationAware, IHandle<EditorClosedNotification<PrefabProject>>
    {
        private readonly ILogger logger = Log.ForContext<PrefabExplorerViewModel>();
        private readonly IWindowManager windowManager;
        private readonly IEventAggregator eventAggregator;    
        private readonly IVersionManagerFactory versionManagerFactory;
        private readonly IApplicationDataManager applicationDataManager;          
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
            IVersionManagerFactory versionManagerFactory, IApplicationDataManager applicationDataManager,
            IPrefabBuilderFactory prefabBuilderFactory)
        {
            this.DisplayName = "Prefab Explorer";
            this.eventAggregator = Guard.Argument(eventAggregator).NotNull().Value;
            this.eventAggregator.SubscribeOnPublishedThread(this);
            this.prefabBuilderFactory = Guard.Argument(prefabBuilderFactory).NotNull().Value;
            this.windowManager = Guard.Argument(windowManager).NotNull().Value;
            this.versionManagerFactory = Guard.Argument(versionManagerFactory).NotNull().Value;
            this.applicationDataManager = Guard.Argument(applicationDataManager).NotNull().Value;
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
            if(!string.IsNullOrEmpty(selectedScreen))
            {
                var prefabsForSelectedScreen = LoadPrefabs(this.ActiveApplication, selectedScreen);
                this.Prefabs.Clear();
                this.Prefabs.AddRange(prefabsForSelectedScreen);
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
                    var prefabs = this.applicationDataManager.GetPrefabsForScreen(applicationDescriptionViewModel.Model, screenName).ToList();
                    foreach (var prefab in prefabs)
                    {
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
            var moveToScreenViewModel = new MoveToScreenViewModel(prefabProject.PrefabName, this.ScreenCollection.Screens, this.ScreenCollection.SelectedScreen);
            var result = await windowManager.ShowDialogAsync(moveToScreenViewModel);
            if (result.GetValueOrDefault())
            {
                this.ActiveApplication.MovePrefabToScreen(prefabProject, this.ScreenCollection.SelectedScreen, moveToScreenViewModel.SelectedScreen);
                await this.applicationDataManager.AddOrUpdateApplicationAsync(this.ActiveApplication.Model);
                this.Prefabs.Remove(prefabProject);
                logger.Information("Moved prefab : {0} from screen {1} to {2} for application {3}", prefabProject.PrefabName, this.ScreenCollection.SelectedScreen, moveToScreenViewModel.SelectedScreen, this.ActiveApplication.ApplicationName);
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
            try
            {
                Guard.Argument(entity).NotNull();

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
                logger.Error(ex, ex.Message);
            }
        }

        private readonly object locker = new();
        /// <summary>
        /// Open Prefab process for editing in process designer
        /// </summary>
        /// <param name="prefabToEdit"></param>
        public async Task EditPrefab(PrefabProjectViewModel prefabToEdit)
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

                var versionPicker = new PrefabVersionPickerViewModel(prefabToEdit.PrefabProject);
                var result = await windowManager.ShowDialogAsync(versionPicker);
                if (result.HasValue && result.Value)
                {
                    var versionToEdit = versionPicker.SelectedVersion;
                    var editorFactory = IoC.Get<IEditorFactory>();
                    var prefabEditor = editorFactory.CreatePrefabEditor();
                    prefabEditor.DoLoad(prefabToEdit.PrefabProject, versionToEdit);
                    await this.eventAggregator.PublishOnUIThreadAsync(new ActivateScreenNotification() { ScreenToActivate = prefabEditor as IScreen });
                }
                else
                {
                    prefabToEdit.IsOpenInEditor = false;
                }
            }
            catch (Exception ex)
            {
                prefabToEdit.IsOpenInEditor = false;
                logger.Error(ex, ex.Message);
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
            try
            {
                var deployPrefabViewModel = versionManagerFactory.CreatePrefabVersionManager(targetPrefab.PrefabProject);                  
                await windowManager.ShowDialogAsync(deployPrefabViewModel);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
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
                var project = this.Prefabs.FirstOrDefault(a => a.PrefabId.Equals(message.Project.PrefabId));
                if (project != null)
                {
                    project.IsOpenInEditor = false;                   
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
            await Task.CompletedTask;
        }

        #endregion Filter      
    }
}
