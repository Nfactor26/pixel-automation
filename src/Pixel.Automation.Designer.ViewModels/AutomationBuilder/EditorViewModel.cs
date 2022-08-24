using Caliburn.Micro;
using Dawn;
using GongSolutions.Wpf.DragDrop;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Core.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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

        public EditorViewModel(IEventAggregator globalEventAggregator, IWindowManager windowManager, ISerializer serializer,
            IEntityManager entityManager, IProjectManager projectManager, IScriptExtactor scriptExtractor, IVersionManagerFactory versionManagerFactory,
            IDropTarget componentDropHandler, ApplicationSettings applicationSettings)
        { 
            this.globalEventAggregator = Guard.Argument(globalEventAggregator, nameof(globalEventAggregator)).NotNull().Value;
            this.globalEventAggregator.SubscribeOnPublishedThread(this);

            this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;
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

        public void DeleteComponent(ComponentViewModel componentViewModel)
        {
            //TODO : Disable delete button on the root entity
            if (componentViewModel.Model.Tag.Equals("Root"))
            {
                logger.Warning("Root entity can't be deleted");               
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

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Are you sure you want to delete this Component?");
            if(scripts?.Any() ?? false)
            {
                sb.AppendLine();
                sb.AppendLine("Below scripts will be deleted : ");
                foreach (var script in scripts)
                {
                    sb.AppendLine(script.ScriptName);
                }
            }            

            MessageBoxResult result = MessageBox.Show(sb.ToString(), "Delete Component", MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK)
            {
                return;
            }           

            if (scripts?.Any() ?? false)
            {
                foreach (var script in scripts)
                {
                    try
                    {
                        File.Delete(Path.Combine(this.EntityManager.GetCurrentFileSystem().WorkingDirectory, script.ScriptName));
                        logger.Information($"Deleted script file {script.ScriptName}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"There was an error while trying to delete file : {script.ScriptName}", ex);                        
                    }
                }
            }

            componentViewModel.Parent.RemoveComponent(componentViewModel);
            if (componentViewModel.Model is IControlEntity controlEntity)
            {
                UpdateControlReferences(controlEntity);               
            }
            logger.Information($"Component : {componentViewModel.Model.Name} has been deleted");         
        }

        private void UpdateControlReferences(IControlEntity controlEntity)
        {
            var fileSystem = this.projectManager.GetProjectFileSystem();
            var controlReferences = fileSystem.LoadFile<ControlReferences>(fileSystem.ControlReferencesFile);
            if (controlReferences.HasReference(controlEntity.ControlId))
            {
                controlReferences.RemoveControlReference(controlReferences.GetControlReference(controlEntity.ControlId));
            }
            fileSystem.SaveToFile<ControlReferences>(controlReferences, fileSystem.ControlReferencesFile);
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
                    if(component is ActorComponent asyncActorComponent)
                    {
                        await asyncActorComponent.ActAsync();
                    }
                    else if (component is IEntityProcessor entityProcessor)
                    {
                        await entityProcessor.BeginProcessAsync();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, ex.Message);
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

        public void ResetProcessorComponents(IComponent component)
        {
            Task componentRunner = new Task(() =>
            {
                try
                {
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
                    logger.Error(ex, ex.Message);
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

        #endregion Manage Components

        #region Automation Project

        public abstract Task DoSave();

        public abstract Task ManageProjectVersionAsync();

        public abstract Task EditDataModelAsync();

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

        /// <inheritdoc/>      
        public async Task ManageControlReferencesAsync()
        {
            try
            {
                var versionManager = this.versionManagerFactory.CreateControlReferenceManager(this.EntityManager.GetCurrentFileSystem());
                await this.windowManager.ShowDialogAsync(versionManager);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }


        #endregion Automation Project

        #region Utilities    

        public void ZoomInToEntity(EntityComponentViewModel entityComponentViewModel)
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
                logger.Error("There was an error trying to change active item", ex);
            }
        }

        public void ZoomOutToEntity(EntityComponentViewModel entityComponentViewModel)
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
                logger.Error("There was an error trying to change active item", ex);
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
                var controlEntities = this.EntityManager.RootEntity.GetComponentsOfType<ControlEntity>(Core.Enums.SearchScope.Descendants);
                var controlsToBeReloaded = controlEntities.Where(c => c.ControlId.Equals(updatedControl.ControlId));
                foreach (var control in controlsToBeReloaded)
                {                 
                    control.Reload();
                    logger.Information($"Control : {control.Name} with Id : {control.ControlId} has been reloaded");
                    await Task.CompletedTask;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public async Task HandleAsync(ApplicationUpdatedEventArgs updatedApplication, CancellationToken cancellationToken)
        {
            try
            {
                var applicationEntities = this.EntityManager.RootEntity.GetComponentsOfType<ApplicationEntity>(Core.Enums.SearchScope.Descendants);
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
                logger.Error(ex, ex.Message);
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
                    ZoomOutToEntity(this.BreadCrumbItems.First());
                }
            }
            await Task.CompletedTask;
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
            var fileSystem = this.projectManager.GetProjectFileSystem();
            ControlReferences controlReferences = File.Exists(fileSystem.ControlReferencesFile) ? fileSystem.LoadFile<ControlReferences>(fileSystem.ControlReferencesFile) : new ControlReferences();
            //Even if we drop a new version, we won't update the versoin in ControlReferences file. Version needs to be manually changed using control reference manager.
            if (!controlReferences.HasReference(controlDescription.ControlId))
            {
                controlReferences.AddControlReference(new ControlReference(controlDescription.ApplicationId, controlDescription.ControlId, controlDescription.Version));
            }
            fileSystem.SaveToFile<ControlReferences>(controlReferences, Path.GetDirectoryName(fileSystem.ControlReferencesFile), Path.GetFileName(fileSystem.ControlReferencesFile));
            await Task.CompletedTask;
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
