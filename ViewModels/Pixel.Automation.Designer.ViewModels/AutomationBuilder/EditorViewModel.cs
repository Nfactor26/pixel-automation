﻿using Caliburn.Micro;
using Dawn;
using GongSolutions.Wpf.DragDrop;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Args;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder
{
    public  abstract class EditorViewModel : ScreenBase, IEditor, IDisposable
    {
        #region data members

        private readonly ILogger logger = Log.ForContext<EditorViewModel>();

        protected readonly IEventAggregator globalEventAggregator;
        protected readonly ISerializer serializer;
        protected readonly IScriptExtactor scriptExtractor;
        protected readonly ApplicationSettings applicationSettings;
     
        public IEntityManager EntityManager { get; private set; }

        public IObservableCollection<IToolBox> Tools { get; } = new BindableCollection<IToolBox>();
   
        public IDropTarget ComponentDropHandler { get; protected set; }        
      
        public BindableCollection<Entity> WorkFlowRoot { get; set; } = new BindableCollection<Entity>();


        #endregion data members

        #region constructor

        public EditorViewModel(IEventAggregator globalEventAggregator, ISerializer serializer,
            IEntityManager entityManager, IScriptExtactor scriptExtractor,  IReadOnlyCollection<IToolBox> tools, 
            IDropTarget componentDropHandler, ApplicationSettings applicationSettings)
        {          
            Guard.Argument(tools, nameof(tools)).NotNull().NotEmpty();

            this.globalEventAggregator = Guard.Argument(globalEventAggregator, nameof(globalEventAggregator)).NotNull().Value;
            this.globalEventAggregator.SubscribeOnUIThread(this);

            this.EntityManager = Guard.Argument(entityManager, nameof(entityManager)).NotNull().Value;
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.scriptExtractor = Guard.Argument(scriptExtractor, nameof(scriptExtractor)).NotNull().Value;
            this.ComponentDropHandler = Guard.Argument(componentDropHandler, nameof(componentDropHandler)).NotNull().Value;
            this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
            this.Tools.AddRange(tools);  
         
        }

        #endregion constructor

        #region Manage Components

        public virtual void DeleteComponent(IComponent component)
        {
            //TODO : Disable delete button on the root entity
            if (component.Tag.Equals("Root"))
            {
                logger.Warning("Root entity can't be deleted");
                return;
            }

            IEnumerable<Editor.Core.ViewModels.ScriptStatus> scripts = default;
            if (component is Entity entity)
            {
                scripts = this.scriptExtractor.ExtractScripts(entity).ToList();
            }
            else
            {
                scripts = this.scriptExtractor.ExtractScripts(component).ToList();
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Are you sure you want to delete this Component?");
            if(scripts?.Any() ?? false)
            {
                sb.AppendLine();
                sb.AppendLine("Following scripts will be deleted?");
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
                        logger.Warning($"There was an error while trying to delete file : {script.ScriptName}");
                        logger.Error(ex.Message, ex);
                    }
                }
            }            

            if (component.Parent != null)
            {
               if(component is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                component.Parent.RemoveComponent(component);
                logger.Information($"Component : {component.Name} has been deleted");
                return;
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

        public void RunComponent(IComponent component)
        {
            Task componentRunner = new Task(async () =>
            {
                try
                {
                    isRunInProgress = true;
                    NotifyOfPropertyChange(() => CanRunComponent);
                    NotifyOfPropertyChange(() => CanResetProcessorComponents);

                    if (component is ActorComponent actorComponent)
                    {
                        actorComponent.Act();
                    }
                    else if(component is AsyncActorComponent asyncActorComponent)
                    {
                        await asyncActorComponent.ActAsync();
                    }
                    else if (component is IEntityProcessor entityProcessor)
                    {
                        await entityProcessor.BeginProcess();
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

        public abstract Task Manage();

        public abstract Task EditDataModel();

        protected void UpdateWorkFlowRoot()
        {
            this.WorkFlowRoot.Clear();
            this.WorkFlowRoot.Add(this.EntityManager.RootEntity);
            this.BreadCrumbItems.Clear();
            this.BreadCrumbItems.Add(this.EntityManager.RootEntity);
        }


        #endregion Automation Project

        #region Utilities

        public BindableCollection<IComponent> BreadCrumbItems { get; set; } = new BindableCollection<IComponent>();
     
        public void ZoomInToEntity(Entity entity)
        {
            if (this.BreadCrumbItems.Contains(entity))
            {
                return;
            }
            this.BreadCrumbItems.Add(entity);
            this.WorkFlowRoot[0] = entity;
        }

        public void ZoomOutToEntity(Entity entity)
        {
            while (BreadCrumbItems.Last() != entity)
            {
                BreadCrumbItems.RemoveAt(BreadCrumbItems.Count() - 1);
            }
            this.WorkFlowRoot[0] = entity;
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

        public async void SetSelectedItem(Object obj)
        {
            if(obj is GroupEntity groupEntity)
            {
                await globalEventAggregator.PublishOnUIThreadAsync(new PropertyGridObjectEventArgs(groupEntity.GroupActor));                
            }
            else
            {
                await globalEventAggregator.PublishOnUIThreadAsync(new PropertyGridObjectEventArgs(obj));
            }
        }

        #endregion property grid

        #region IDisposable

        public  void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if(isDisposing)
            {              
                this.Tools.Clear();
                this.WorkFlowRoot.Clear();               
            }
        }

        #endregion IDisposable
    }
}
