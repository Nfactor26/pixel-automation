using Caliburn.Micro;
using Dawn;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Contracts;
using Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder;
using Pixel.Automation.Core;
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
    public class PrefabExplorerViewModel : Screen, IApplicationAware
    {
        private readonly ILogger logger = Log.ForContext<PrefabExplorerViewModel>();
        private readonly IWindowManager windowManager;
        private readonly IEventAggregator eventAggregator;    
        private readonly IVersionManagerFactory versionManagerFactory;
        private readonly IApplicationDataManager applicationDataManager;   
        private ApplicationDescriptionViewModel activeApplication;
        private IPrefabBuilderFactory prefabBuilderFactory;
       
        public BindableCollection<PrefabProjectViewModel> Prefabs { get; set; } = new ();
      
        public PrefabProjectViewModel SelectedPrefab { get; set; }

        public PrefabDragHandler PrefabDragHandler { get; private set; } = new ();

        public PrefabExplorerViewModel(IEventAggregator eventAggregator, IWindowManager windowManager,
            IVersionManagerFactory versionManagerFactory, IApplicationDataManager applicationDataManager,
            IPrefabBuilderFactory prefabBuilderFactory)
        {
            this.DisplayName = "Prefab Explorer";
            this.eventAggregator = Guard.Argument(eventAggregator).NotNull().Value;
            this.prefabBuilderFactory = Guard.Argument(prefabBuilderFactory).NotNull().Value;
            this.windowManager = Guard.Argument(windowManager).NotNull().Value;
            this.versionManagerFactory = Guard.Argument(versionManagerFactory).NotNull().Value;
            this.applicationDataManager = Guard.Argument(applicationDataManager).NotNull().Value;
            CreateCollectionView();
        }

        /// <inheritdoc/>
        public void SetActiveApplication(ApplicationDescriptionViewModel applicationDescriptionViewModel)
        {
            this.activeApplication = applicationDescriptionViewModel;          
            this.Prefabs.Clear();
            if(this.activeApplication != null)
            {
                LoadPrefabs(applicationDescriptionViewModel);
                this.Prefabs.AddRange(this.activeApplication.PrefabsCollection);
            }                     
        }

        private void LoadPrefabs(ApplicationDescriptionViewModel applicationDescriptionViewModel)
        {
            if(applicationDescriptionViewModel.PrefabsCollection.Count() == 0)
            {
                foreach(var prefab in applicationDataManager.GetAllPrefabs(applicationDescriptionViewModel.ApplicationId))
                {
                    applicationDescriptionViewModel.AddPrefab(new PrefabProjectViewModel(prefab));
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
            try
            {
                Guard.Argument(entity).NotNull();

                var prefabBuilder = prefabBuilderFactory.CreatePrefabBuilder();
                prefabBuilder.Initialize(this.activeApplication, entity);
                var result = await windowManager.ShowDialogAsync(prefabBuilder);
                if (result.HasValue && result.Value)
                {
                    var createdPrefab = await prefabBuilder.SavePrefabAsync();
                    var prefabViewModel = new PrefabProjectViewModel(createdPrefab);
                    this.Prefabs.Add(prefabViewModel);
                    this.activeApplication.AddPrefab(prefabViewModel);
                    await this.applicationDataManager.AddOrUpdateApplicationAsync(this.activeApplication.Model);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

       
        /// <summary>
        /// Open Prefab process for editing in process designer
        /// </summary>
        /// <param name="prefabToEdit"></param>
        public void EditPrefab(PrefabProjectViewModel prefabToEdit)
        {
            try
            {
                var editorFactory = IoC.Get<IEditorFactory>();              
                var prefabEditor = editorFactory.CreatePrefabEditor();              
                prefabEditor.DoLoad(prefabToEdit.PrefabProject);               
                this.eventAggregator.PublishOnUIThreadAsync(new ActivateScreenNotification() { ScreenToActivate = prefabEditor as IScreen });               
            }
            catch (Exception ex)
            {
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

        #endregion Filter      
    }
}
