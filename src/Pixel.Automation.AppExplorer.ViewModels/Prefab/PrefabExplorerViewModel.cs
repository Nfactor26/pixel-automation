﻿using Caliburn.Micro;
using Dawn;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.DragDropHandler;
using Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Core.Notfications;
using Pixel.Persistence.Services.Client;
using Serilog;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

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
        private readonly IVersionManagerFactory versionManagerFactory;
        private readonly IApplicationDataManager applicationDataManager;   
        private ApplicationDescriptionViewModel activeApplication;
        private IPrefabBuilderViewModelFactory prefabBuilderFactory;
       
        public BindableCollection<PrefabProject> Prefabs { get; set; } = new BindableCollection<PrefabProject>();
      
        public PrefabProject SelectedPrefab { get; set; }

        public PrefabDragHandler PrefabDragHandler { get; private set; } = new PrefabDragHandler();

        public PrefabExplorerViewModel(IEventAggregator eventAggregator, IWindowManager windowManager,
            ISerializer serializer, IVersionManagerFactory versionManagerFactory, IApplicationDataManager applicationDataManager,
            IPrefabBuilderViewModelFactory prefabBuilderFactory)
        {
            this.eventAggregator = Guard.Argument(eventAggregator).NotNull().Value;
            this.prefabBuilderFactory = Guard.Argument(prefabBuilderFactory).NotNull().Value;
            this.windowManager = Guard.Argument(windowManager).NotNull().Value;
            this.serializer = Guard.Argument(serializer).NotNull().Value;
            this.versionManagerFactory = Guard.Argument(versionManagerFactory).NotNull().Value;
            this.applicationDataManager = Guard.Argument(applicationDataManager).NotNull().Value;
            CreateCollectionView();
        }

        public void SetActiveApplication(ApplicationDescriptionViewModel applicationDescriptionViewModel)
        {
            this.activeApplication = applicationDescriptionViewModel;          
            this.Prefabs.Clear();
            if(applicationDescriptionViewModel != null)
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
                    applicationDescriptionViewModel.AddPrefab(prefab);
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
        public void EditPrefab(PrefabProject prefabToEdit)
        {
            try
            {
                var editorFactory = IoC.Get<IEditorFactory>();              
                var prefabEditor = editorFactory.CreatePrefabEditor();              
                prefabEditor.DoLoad(prefabToEdit);               
                this.eventAggregator.PublishOnUIThreadAsync(new ActivateScreenNotification() { ScreenToActivate = prefabEditor as IScreen });               
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public async Task ManagePrefab(PrefabProject targetPrefab)
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

        /// <summary>
        /// Broadcast a FilterTestMessage which is processed by Test explorer view to filter and show only those test cases
        /// which uses this prefab
        /// </summary>
        /// <param name="targetPrefab"></param>
        /// <returns></returns>
        public async Task ShowUsage(PrefabProject targetPrefab)
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
            groupedItems.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PrefabProject.GroupName)));
            groupedItems.SortDescriptions.Add(new SortDescription(nameof(PrefabProject.GroupName), ListSortDirection.Ascending));
            groupedItems.SortDescriptions.Add(new SortDescription(nameof(PrefabProject.PrefabName), ListSortDirection.Ascending));
            groupedItems.Filter = new Predicate<object>((a) =>
            {
                return (a as PrefabProject).PrefabName.ToLower().Contains(this.filterText.ToLower());
            });
        }

        #endregion Filter

        public event EventHandler<PrefabProject> PrefabCreated = delegate { };
        protected virtual void OnPrefabCreated(PrefabProject createdPrefab)
        {
            this.PrefabCreated(this, createdPrefab);
        }
      
    }
}
