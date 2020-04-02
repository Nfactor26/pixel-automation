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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        private ApplicationDescription activeApplication;
        private PrefabBuilderViewModel prefabBuilder;

        public BindableCollection<PrefabDescription> Prefabs { get; set; } = new BindableCollection<PrefabDescription>();
      
        public PrefabDescription SelectedPrefab { get; set; }

        public PrefabDragHandler PrefabDragHandler { get; private set; } = new PrefabDragHandler();

        public PrefabExplorerViewModel(IEventAggregator eventAggregator, IWindowManager windowManager, ISerializer serializer, PrefabBuilderViewModel prefabBuilder)
        {
            this.eventAggregator = eventAggregator;
            this.prefabBuilder = prefabBuilder;
            this.windowManager = windowManager;
            this.serializer = serializer;       

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
                    string prefabFile = Path.Combine("ApplicationsRepository", application.ApplicationId, "Prefabs", prefabId, "PrefabDescription.dat");
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

            this.prefabBuilder.Initialize(this.activeApplication, entity);
            var result = await windowManager.ShowDialogAsync(this.prefabBuilder);
            if (result.HasValue && result.Value)
            {
                var createdPrefab = this.prefabBuilder.SavePrefab();                 
                this.Prefabs.Add(createdPrefab);
                OnPrefabCreated(createdPrefab);
            }           
        }

        /// <summary>
        /// Delete the prefab 
        /// </summary>
        /// <param name="prefabToDelete"></param>
        public void DeletePrefab(PrefabDescription prefabToDelete)
        {
            Guard.Argument(prefabToDelete).NotNull();

            string prefabDirectory = Path.Combine("ApplicationsRepository", prefabToDelete.ApplicationId, "Prefabs", prefabToDelete.PrefabId);
            this.Prefabs.Remove(prefabToDelete);
            OnPrefabDeleted(prefabToDelete);
            try
            {
                //Will throw if dll is in use 
                Directory.Delete(prefabDirectory, true);
            }
            catch 
            {
            
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
                Log.Error(ex, ex.Message);
            }
        }

        internal void SavePrefabDescription(PrefabDescription prefabToSave)
        {
            Guard.Argument(prefabToSave).NotNull();

            string prefabDescriptionFile = Path.Combine("ApplicationsRepository", prefabToSave.ApplicationId, "Prefabs", prefabToSave.PrefabId, "PrefabDescription.dat");
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
                CanEdit = !CanEdit;
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
                    string newName = (context.Source as System.Windows.Controls.TextBox).Text;
                    prefabItem.PrefabName = newName;
                    CanEdit = false;
                    //OnCollectionChanged();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
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

        public event EventHandler<PrefabDescription> PrefabDeleted = delegate { };
        protected virtual void OnPrefabDeleted(PrefabDescription deletedPrefab)
        {
            this.PrefabCreated(this, deletedPrefab);
        }
    }
}
