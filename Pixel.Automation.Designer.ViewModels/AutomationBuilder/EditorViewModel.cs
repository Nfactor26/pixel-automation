using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Args;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Designer.ViewModels.DragDropHandlers;
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
        readonly IComponentBox componentToolBox;        

        public EntityManager EntityManager { get; }

        public IObservableCollection<IToolBox> Tools { get; } = new BindableCollection<IToolBox>();
   
        public ComponentDropHandler ComponentDropHandler { get; protected set; }
        
        BindableCollection<Entity> workFlowRoot = new BindableCollection<Entity>();
        public BindableCollection<Entity> WorkFlowRoot
        {
            get => workFlowRoot;
            protected set
            {
                this.workFlowRoot = value;
                NotifyOfPropertyChange(() => WorkFlowRoot);
            }

        }

        #endregion data members

        #region constructor
        public EditorViewModel(IEventAggregator globalEventAggregator, IServiceResolver serviceResolver,
            ISerializer serializer, IScriptExtactor scriptExtractor,
            IToolBox[] toolBoxes)
        {
            this.globalEventAggregator = globalEventAggregator;
            this.globalEventAggregator.SubscribeOnUIThread(this);

            this.serializer = serializer;
            this.scriptExtractor = scriptExtractor;

            this.Tools.AddRange(toolBoxes);
            foreach (var item in Tools)
            {
                if (item is IComponentBox)
                {
                    componentToolBox = item as IComponentBox;
                    continue;
                }               
            }

            this.EntityManager = new EntityManager(serviceResolver);
            this.ComponentDropHandler = new ComponentDropHandler();
        }

        #endregion constructor

        #region Manage Components

        public void AddComponent(Entity parent, IComponent childComponent)
        {

        }

        public void DeleteComponent(IComponent component)
        {
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

            if (component.Tag.Equals("Root"))
            {
                logger.Warning("Root entity can't be deleted");
                return;
            }

            if (scripts?.Any() ?? false)
            {
                foreach (var script in scripts)
                {
                    File.Delete(Path.Combine(this.EntityManager.GetCurrentFileSystem().WorkingDirectory, script.ScriptName));
                    logger.Information($"Deleted script file {script.ScriptName}");
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

                    if (component is ActorComponent)
                    {
                        (component as ActorComponent).Act();
                    }
                    else if (component is IEntityProcessor)
                    {
                        await (component as IEntityProcessor).BeginProcess();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, ex.Message);
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
                    Log.Error(ex, ex.Message);
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


        public abstract Task EditDataModel();
       

        public void DoUnload()
        {
            this.EntityManager?.Dispose();
        }


        #endregion Automation Project

        #region Utilities

        public BindableCollection<IComponent> BreadCrumbItems { get; set; } = new BindableCollection<IComponent>();
     
        public void ZoomInToEntity(Entity entity)
        {
            if (this.BreadCrumbItems.Contains(entity))
                return;
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

        #region Save project

        public abstract void DoSave();


        public abstract Task Manage();
       

        #endregion Save project

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

        public abstract Task CloseAsync();

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
                this.DoUnload();
            }
        }

        #endregion IDisposable
    }
}
