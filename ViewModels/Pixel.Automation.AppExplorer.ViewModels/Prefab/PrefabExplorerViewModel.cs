using Caliburn.Micro;
using Dawn;
using Pixel.Automation.AppExplorer.ViewModels.DragDropHandler;
using Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Serilog;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using IPrefabEditorFactory = Pixel.Automation.Editor.Core.Interfaces.IEditorFactory;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefab
{
    /// <summary>
    /// PrefabExplorer allows creating prefabs which are reusable components for a given application.
    /// </summary>
    public class PrefabExplorerViewModel : Screen
    {
        private readonly ILogger logger = Log.ForContext<PrefabExplorerViewModel>();
        private readonly ISerializer serializer;
        private readonly IWindowManager windowManager;
        private readonly IEventAggregator eventAggregator;
        private readonly IPrefabEditorFactory editorFactory;
        private readonly IVersionManagerFactory versionManagerFactory;
        private ApplicationDescription activeApplication;
        private IPrefabBuilderViewModelFactory prefabBuilderFactory;
       
        public BindableCollection<PrefabDescription> Prefabs { get; set; } = new BindableCollection<PrefabDescription>();
      
        public PrefabDescription SelectedPrefab { get; set; }

        public PrefabDragHandler PrefabDragHandler { get; private set; } = new PrefabDragHandler();

        public PrefabExplorerViewModel(IEventAggregator eventAggregator, IWindowManager windowManager,
            ISerializer serializer, IVersionManagerFactory versionManagerFactory, IPrefabEditorFactory editorFactory,
            IPrefabBuilderViewModelFactory prefabBuilderFactory)
        {
            this.eventAggregator = eventAggregator;
            this.prefabBuilderFactory = prefabBuilderFactory;
            this.windowManager = windowManager;
            this.serializer = serializer;
            this.editorFactory = editorFactory;
            this.versionManagerFactory = versionManagerFactory;          
            CreateCollectionView();
        }

        public void SetActiveApplication(ApplicationDescription application)
        {
            Guard.Argument(application).NotNull();

            this.activeApplication = application;
            LoadPrefabs(application);
            this.Prefabs.Clear();
            this.Prefabs.AddRange(this.activeApplication.PrefabsCollection);            
        }


        private void LoadPrefabs(ApplicationDescription application)
        {
            if(application.PrefabsCollection.Count() == 0)
            {
                foreach(var prefabId in application.AvailablePrefabs)
                {
                    string prefabFile = Path.Combine("Applications", application.ApplicationId, "Prefabs", prefabId, "PrefabDescription.dat");
                    if(File.Exists(prefabFile))
                    {
                        PrefabDescription prefabDescription = serializer.Deserialize<PrefabDescription>(prefabFile);
                        application.PrefabsCollection.Add(prefabDescription);
                        continue;
                    }
                    logger.Warning("Prefab file : {0} doesn't exist", prefabFile);
                }
            }       
        }

        public async void CreatePrefab(Entity entity)
        {
            Guard.Argument(entity).NotNull();

            var prefabBuilder = prefabBuilderFactory.CreatePrefabBuilderViewModel();
            prefabBuilder.Initialize(this.activeApplication, entity);
            var result = await windowManager.ShowDialogAsync(prefabBuilder);
            if (result.HasValue && result.Value)
            {
                var createdPrefab = await prefabBuilder.SavePrefabAsync();                 
                this.Prefabs.Add(createdPrefab);           
                OnPrefabCreated(createdPrefab);
            }           
        }

       
        /// <summary>
        /// Open Prefab for Edit in Designer
        /// </summary>
        /// <param name="prefabToEdit"></param>
        public void EditPrefab(PrefabDescription prefabToEdit)
        {
            try
            {            
                var prefabEditor = this.editorFactory.CreatePrefabEditor();              
                prefabEditor.DoLoad(prefabToEdit);               
                this.eventAggregator.PublishOnUIThreadAsync(new ActivateScreenNotification() { ScreenToActivate = prefabEditor as IScreen });               
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public async Task ManagePrefab(PrefabDescription targetPrefab)
        {
            try
            {
                var deployPrefabViewModel = versionManagerFactory.CreatePrefabVersionManager(targetPrefab);                  
                await windowManager.ShowDialogAsync(deployPrefabViewModel);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
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
            groupedItems.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PrefabDescription.GroupName)));
            groupedItems.SortDescriptions.Add(new SortDescription(nameof(PrefabDescription.GroupName), ListSortDirection.Ascending));
            groupedItems.SortDescriptions.Add(new SortDescription(nameof(PrefabDescription.PrefabName), ListSortDirection.Ascending));
            groupedItems.Filter = new Predicate<object>((a) =>
            {
                return (a as PrefabDescription).PrefabName.ToLower().Contains(this.filterText.ToLower());
            });
        }

        #endregion Filter

        public event EventHandler<PrefabDescription> PrefabCreated = delegate { };
        protected virtual void OnPrefabCreated(PrefabDescription createdPrefab)
        {
            this.PrefabCreated(this, createdPrefab);
        }
      
    }
}
