using Caliburn.Micro;
using Dawn;
using Pixel.Automation.AppExplorer.ViewModels.DragDropHandler;
using Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefab
{
    public class PrefabExplorerViewModel : Screen
    {
        private readonly ILogger logger = Log.ForContext<PrefabExplorerViewModel>();
        private readonly ISerializer serializer;
        private readonly IWindowManager windowManager;
        private readonly IEventAggregator eventAggregator;
        private readonly IWorkspaceManagerFactory workspaceManagerFactory;
        private ApplicationDescription activeApplication;
        private IPrefabBuilderViewModelFactory prefabBuilderFactory;

        public BindableCollection<PrefabDescription> Prefabs { get; set; } = new BindableCollection<PrefabDescription>();
      
        public PrefabDescription SelectedPrefab { get; set; }

        public PrefabDragHandler PrefabDragHandler { get; private set; } = new PrefabDragHandler();

        public PrefabExplorerViewModel(IEventAggregator eventAggregator, IWindowManager windowManager,
            ISerializer serializer, IWorkspaceManagerFactory workspaceManagerFactory,
            IPrefabBuilderViewModelFactory prefabBuilderFactory)
        {
            this.eventAggregator = eventAggregator;
            this.prefabBuilderFactory = prefabBuilderFactory;
            this.windowManager = windowManager;
            this.serializer = serializer;
            this.workspaceManagerFactory = workspaceManagerFactory;
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
                var createdPrefab = prefabBuilder.SavePrefab();                 
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
                IPrefabEditor prefabEditor = IoC.Get<IPrefabEditor>(); //TODO : Pass a factory for creating PrefabEditors and use that
                prefabEditor.DoLoad(prefabToEdit);               
                this.eventAggregator.PublishOnUIThreadAsync(new ActivateScreenNotification() { ScreenToActivate = prefabEditor as IScreen });
                prefabEditor.PrefabUpdated += OnPrefabUpdated;
                prefabEditor.EditorClosing += OnPrefabEditorClosing;
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
                PrefabVersionManagerViewModel deployPrefabViewModel = new PrefabVersionManagerViewModel(targetPrefab, this.workspaceManagerFactory, this.serializer);
                var result = await windowManager.ShowDialogAsync(deployPrefabViewModel);
                if (result.GetValueOrDefault())
                {
                    SavePrefabDescription(targetPrefab);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        internal void SavePrefabDescription(PrefabDescription prefabToSave)
        {
            Guard.Argument(prefabToSave).NotNull();

            string prefabDescriptionFile = Path.Combine("Applications", prefabToSave.ApplicationId, "Prefabs", prefabToSave.PrefabId, "PrefabDescription.dat");
            serializer.Serialize<PrefabDescription>(prefabDescriptionFile, prefabToSave);
            if(!this.Prefabs.Contains(prefabToSave))
            {
                this.Prefabs.Add(prefabToSave);               
            }            
        }

        private void OnPrefabEditorClosing(object sender, EditorClosingEventArgs e)
        {
            IPrefabEditor prefabEditor = sender as IPrefabEditor;
            prefabEditor.PrefabUpdated -= OnPrefabUpdated;
            prefabEditor.EditorClosing -= OnPrefabEditorClosing;
        }

        private void OnPrefabUpdated(object sender, PrefabUpdatedEventArgs e)
        {
            SavePrefabDescription(e.TargetPrefab);           
        }

        #region Edit ControlToolBoxItem


        bool canEdit;
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

        public void ToggleRename(PrefabDescription prefabItem)
        {
            if (SelectedPrefab == prefabItem)
            {
                CanEdit = !CanEdit;
            }
        }

        /// <summary>
        /// Update the name of ControlToolBoxItem
        /// </summary>
        /// <param name="context"></param>
        /// <param name="controlToRename"></param>
        public void RenameControl(ActionExecutionContext context, PrefabDescription prefabItem)
        {
            try
            {
                var keyArgs = context.EventArgs as KeyEventArgs;
                if (keyArgs != null && keyArgs.Key == Key.Enter)
                {
                    string previousName = prefabItem.PrefabName;
                    string newName = (context.Source as System.Windows.Controls.TextBox).Text;
                    prefabItem.PrefabName = newName;
                    CanEdit = false;
                    SavePrefabDescription(prefabItem);
                    logger.Information($"Prefab : {previousName} renamed to : {newName}");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                CanEdit = false;
            }
        }


        #endregion Edit ControlToolBoxItem

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
