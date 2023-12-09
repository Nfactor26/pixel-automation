using Caliburn.Micro;
using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Core.ViewModels;
using Serilog;
using System.Windows;
using IDropTarget = GongSolutions.Wpf.DragDrop.IDropTarget;
using MessageBox = System.Windows.MessageBox;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder
{
    public  abstract class EditorViewModel : ScreenBase, IEditor, IDisposable, IHandle<ControlUpdatedEventArgs>, IHandle<ApplicationUpdatedEventArgs>,
        IHandle<TestEntityRemovedEventArgs>, IHandle<ControlAddedEventArgs>
    {
        #region data members

        private readonly ILogger logger = Log.ForContext<EditorViewModel>();

        protected readonly IEventAggregator globalEventAggregator;
        protected readonly ISerializer serializer;
        protected readonly IScriptExtactor scriptExtractor;
        protected readonly ApplicationSettings applicationSettings;
        protected readonly IVersionManagerFactory versionManagerFactory;
        protected readonly IWindowManager windowManager;
        protected readonly INotificationManager notificationManager;
        private readonly IProjectManager projectManager;
      
        public IEntityManager EntityManager { get; private set; }

        public IDropTarget ComponentDropHandler { get; protected set; }

        private EntityComponentViewModel activeItem;
        public EntityComponentViewModel ActiveItem
        {
            get => activeItem;
            set
            {
                activeItem = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<EntityComponentViewModel> WorkFlowRoot { get; private set; } = new ();

        public BindableCollection<EntityComponentViewModel> BreadCrumbItems { get; set; } = new ();

        #endregion data members

        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="globalEventAggregator"></param>
        /// <param name="windowManager"></param>
        /// <param name="notificationManager"></param>
        /// <param name="serializer"></param>
        /// <param name="entityManager"></param>
        /// <param name="projectManager"></param>
        /// <param name="scriptExtractor"></param>
        /// <param name="versionManagerFactory"></param>
        /// <param name="componentDropHandler"></param>
        /// <param name="applicationSettings"></param>
        public EditorViewModel(IEventAggregator globalEventAggregator, IWindowManager windowManager, INotificationManager notificationManager, ISerializer serializer,
            IEntityManager entityManager, IProjectManager projectManager, IScriptExtactor scriptExtractor, IVersionManagerFactory versionManagerFactory,
            IDropTarget componentDropHandler, ApplicationSettings applicationSettings)
        { 
            this.globalEventAggregator = Guard.Argument(globalEventAggregator, nameof(globalEventAggregator)).NotNull().Value;
            this.globalEventAggregator.SubscribeOnPublishedThread(this);

            this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;
            this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;
            this.EntityManager = Guard.Argument(entityManager, nameof(entityManager)).NotNull().Value;
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.projectManager = Guard.Argument(projectManager, nameof(projectManager)).NotNull().Value;
            this.scriptExtractor = Guard.Argument(scriptExtractor, nameof(scriptExtractor)).NotNull().Value;
            this.versionManagerFactory = Guard.Argument(versionManagerFactory, nameof(versionManagerFactory)).Value;
            this.ComponentDropHandler = Guard.Argument(componentDropHandler, nameof(componentDropHandler)).NotNull().Value;
            this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();          
        }

        #endregion constructor

        #region Manage Components

        /// <summary>
        /// Delete a component from the designer
        /// </summary>
        /// <param name="componentViewModel"></param>
        /// <returns></returns>
        public async Task DeleteComponent(ComponentViewModel componentViewModel)
        {          
            try
            {
                Guard.Argument(componentViewModel, nameof(componentViewModel)).NotNull();
                if (componentViewModel.Model.Tag.Equals("Root"))
                {
                    return;
                }

                IEnumerable<ScriptStatus> scripts = default;
                if (componentViewModel.Model is Entity entity)
                {
                    scripts = this.scriptExtractor.ExtractScripts(entity).ToList();
                }
                else
                {
                    scripts = this.scriptExtractor.ExtractScripts(componentViewModel.Model).ToList();
                }
          
                var deleteScriptsViewModel = new DeleteComponentViewModel(componentViewModel, scripts ?? Enumerable.Empty<ScriptStatus>(), 
                    this.projectManager, this.globalEventAggregator);
                var result = await this.windowManager.ShowDialogAsync(deleteScriptsViewModel);
                if (!result.GetValueOrDefault())
                {
                    return;
                }
                logger.Information($"Component : {componentViewModel.Model.Name} has been deleted");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while deleting component : '{0}'", componentViewModel?.ToString());
                await notificationManager.ShowErrorNotificationAsync(ex);
            }    
        }

        private bool isRunInProgress = false;
        public bool CanRunComponent
        {
            get
            {
                return !isRunInProgress;
            }
        }

        public void RunComponent(ComponentViewModel componentViewModel)
        {
            Task componentRunner = new Task(async () =>
            {
                try
                {
                    isRunInProgress = true;
                    NotifyOfPropertyChange(() => CanRunComponent);
                    NotifyOfPropertyChange(() => CanResetProcessorComponents);
                    var component = componentViewModel.Model; 
                    if(component is ActorComponent actorComponent)
                    {
                        await actorComponent.ActAsync();
                    }
                    else if (component is IEntityProcessor entityProcessor)
                    {
                        await entityProcessor.BeginProcessAsync();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to run component : '{0}'", componentViewModel?.Model.ToString());
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
                finally
                {
                    isRunInProgress = false;
                    NotifyOfPropertyChange(() => CanRunComponent);
                    NotifyOfPropertyChange(() => CanResetProcessorComponents);
                }
            });
            componentRunner.Start();
        }

        public bool CanResetProcessorComponents
        {
            get
            {
                return !isRunInProgress;
            }
        }

        public async Task ResetProcessorComponents(IComponent component)
        {
            try
            {
                Guard.Argument(component, nameof(component)).NotNull();
                isRunInProgress = true;
                NotifyOfPropertyChange(() => CanRunComponent);
                NotifyOfPropertyChange(() => CanResetProcessorComponents);

                if (component is IEntityProcessor)
                {
                    (component as IEntityProcessor).ResetComponents();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to reset components");
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
            finally
            {
                isRunInProgress = false;
                NotifyOfPropertyChange(() => CanRunComponent);
                NotifyOfPropertyChange(() => CanResetProcessorComponents);
            }
        }

        #endregion Manage Components

        #region Automation Project

        public abstract Task DoSave();

        public abstract Task EditDataModelAsync();

        public abstract Task EditScriptAsync();

        protected void UpdateWorkFlowRoot()
        {
            this.WorkFlowRoot.Clear();
            this.WorkFlowRoot.Add(new EntityComponentViewModel(this.EntityManager.RootEntity));
            this.BuildWorkFlow(this.WorkFlowRoot[0]);
            this.BreadCrumbItems.Clear();
            this.BreadCrumbItems.Add(this.WorkFlowRoot[0]);
            this.ActiveItem = this.WorkFlowRoot[0];
        }

        protected virtual void BuildWorkFlow(EntityComponentViewModel root)
        {            
            foreach (var component in this.EntityManager.RootEntity.Components)
            {
                root.AddComponent(component);
            }
        }

        /// <summary>
        /// Reload the project while preserving the last editor state e.g. test cases opened earlier
        /// </summary>
        /// <returns></returns>
        protected abstract Task Reload();       

        /// <inheritdoc/>      
        public async Task ManageControlReferencesAsync()
        {
            try
            {
                var versionManager = this.versionManagerFactory.CreateControlReferenceManager(this.projectManager.GetReferenceManager());
                await this.windowManager.ShowDialogAsync(versionManager);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error on control references screen");
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }

        /// <inheritdoc/>      
        public async Task ManageAssemblyReferencesAsync()
        {
            try
            {
                var versionManager = this.versionManagerFactory.CreateAssemblyReferenceManager(this.EntityManager.GetCurrentFileSystem(),
                    this.projectManager.GetReferenceManager());
                var result = await this.windowManager.ShowDialogAsync(versionManager);
                if(result.HasValue && result.Value)
                {
                    await this.Reload();
                }               
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error on asssembly reference screen");
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }


        #endregion Automation Project

        #region Utilities    

        public async Task ZoomInToEntity(EntityComponentViewModel entityComponentViewModel)
        {
            try
            {
                if (this.BreadCrumbItems.Contains(entityComponentViewModel))
                {
                    return;
                }
                List<EntityComponentViewModel> ancestors = new();
                EntityComponentViewModel current = entityComponentViewModel;
                while (current != ActiveItem)
                {
                    ancestors.Add(current);
                    current = current.Parent;
                }
                for (int i = ancestors.Count() - 1; i >= 0; i--)
                {
                    this.BreadCrumbItems.Add(ancestors[i]);
                }
                this.WorkFlowRoot.Clear();
                this.WorkFlowRoot.Add(entityComponentViewModel);
                this.ActiveItem = entityComponentViewModel;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error trying to zoom in to component");
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }

        public async Task ZoomOutToEntity(EntityComponentViewModel entityComponentViewModel)
        {
            try
            {
                while (BreadCrumbItems.Last() != entityComponentViewModel)
                {
                    BreadCrumbItems.RemoveAt(BreadCrumbItems.Count() - 1);
                }
                this.WorkFlowRoot.Clear();
                this.WorkFlowRoot.Add(entityComponentViewModel);
                this.ActiveItem = entityComponentViewModel;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error trying to zoom out of component");
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }

        #endregion Utilities
     
        #region Close Screen

        public override bool CanClose()
        {
            return true;
        }

        public override async void CloseScreen()
        {           
            MessageBoxResult result = MessageBox.Show("Are you sure you want to close? Any unsaved changes will be lost.", "Confirm Close", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                await CloseAsync();
            }
        }

        protected abstract Task CloseAsync();

        #endregion Close Screen

        #region property grid     

        public async Task SetSelectedItem(Object obj)
        {
            if(obj is ComponentViewModel componentViewModel)
            {
                if (componentViewModel.Model is GroupEntity groupEntity)
                {
                    await globalEventAggregator.PublishOnUIThreadAsync(new PropertyGridObjectEventArgs(groupEntity.GroupActor));
                    return;
                }
                await globalEventAggregator.PublishOnUIThreadAsync(new PropertyGridObjectEventArgs(componentViewModel.Model));
            }           
        }

        #endregion property grid

        #region IHandle<T>

        public async Task HandleAsync(ControlUpdatedEventArgs updatedControl, CancellationToken cancellationToken)
        {
            try
            {
                var controlLoader = this.EntityManager.GetServiceOfType<IControlLoader>();
                controlLoader.RemoveFromCache(updatedControl.ControlId);             
                var controlEntities = this.EntityManager.RootEntity.GetComponentsOfType<ControlEntity>(SearchScope.Descendants);
                var controlsToBeReloaded = controlEntities.Where(c => c.ControlId.Equals(updatedControl.ControlId));
                foreach (var control in controlsToBeReloaded)
                {                 
                    control.Refresh();                 
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to handle control updated notification for control : '{0}'", updatedControl?.ControlId);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }

        public async Task HandleAsync(ApplicationUpdatedEventArgs updatedApplication, CancellationToken cancellationToken)
        {
            try
            {
                var applicationEntities = this.EntityManager.RootEntity.GetComponentsOfType<ApplicationEntity>(SearchScope.Descendants);
                var applicationsToReload = applicationEntities.Where(a => a.ApplicationId.Equals(updatedApplication.ApplicationId));
                foreach (var application in applicationsToReload)
                {
                    application.Reload();
                    logger.Information($"Application : {application.Name} with Id : {application.ApplicationId} has been reloaded");
                    await Task.CompletedTask;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to handle application updated notification for application : '{0}'", updatedApplication?.ApplicationId);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }

        /// <summary>
        /// When a test fixture or test case is closed from test explorer, if the test fixture entity or test case entity or any of their descendant
        /// are set as the ActiveItem, we need to remove them as ActiveItem and reset the view to RootEntity.
        /// </summary>
        /// <param name="removedEntity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task HandleAsync(TestEntityRemovedEventArgs removedEntity, CancellationToken cancellationToken)
        {
            foreach(var item in this.BreadCrumbItems)
            {
                if(item.Model == removedEntity.RemovedEntity)
                {
                    await ZoomOutToEntity(this.BreadCrumbItems.First());
                }
            }           
        }

        /// <summary>
        /// Handle notification for a control added event to update the ControlReferences file for project.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task HandleAsync(ControlAddedEventArgs control, CancellationToken cancellationToken)
        {
            var controlDescription = control.Control;
            try
            {             
                var referenceManager = this.projectManager.GetReferenceManager();
                await referenceManager.AddControlReferenceAsync(new ControlReference(controlDescription.ApplicationId, controlDescription.ControlId, controlDescription.Version));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to add control reference for control : {0}", controlDescription?.ControlName);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }  
        }


        #endregion IHandle<T>

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if(isDisposing)
            {                            
                this.WorkFlowRoot.Clear();
                this.ActiveItem = null;
            }
        }
       
        #endregion IDisposable
    }
}
