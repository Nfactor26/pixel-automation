using Caliburn.Micro;
using Dawn;
using GongSolutions.Wpf.DragDrop;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Control;
using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.AppExplorer.ViewModels.PrefabDropHandler;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Components.Sequences;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Core.ViewModels;
using Serilog;
using System.IO;
using System.Windows;
using DragDropEffects = System.Windows.DragDropEffects;
using MessageBox = System.Windows.MessageBox;
using TreeView = System.Windows.Controls.TreeView;

namespace Pixel.Automation.Designer.ViewModels.DragDropHandlers
{
    public class ComponentDropHandler : GongSolutions.Wpf.DragDrop.IDropTarget
    {
        private readonly ILogger logger = Log.ForContext<ComponentDropHandler>();
        private readonly ApplicationSettings applicationSettings;
        private readonly IEventAggregator eventAggregator;

        public ComponentDropHandler(ApplicationSettings applicationSettings, IEventAggregator eventAggregator)
        {
            this.applicationSettings = Guard.Argument(applicationSettings).NotNull().Value;
            this.eventAggregator = Guard.Argument(eventAggregator).NotNull().Value;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            try
            {
                //if source && target are same , do nothing
                if (dropInfo.Data == dropInfo.TargetItem)
                {
                    return;
                }
                
                //Component/Entity is being rearranged i.e. dragged away from its parent Entity to some other Entity
                if (dropInfo.Data is ComponentViewModel sourceItem && dropInfo.TargetItem is ComponentViewModel targetItem)
                {
                    switch (System.Windows.Forms.Control.ModifierKeys)
                    {
                        case System.Windows.Forms.Keys.Alt:
                            //It should not be possible to drag an item to one of it's child
                            var current = targetItem;
                            while(current.Parent != null)
                            {
                                current = current.Parent;
                                if(current == sourceItem)
                                {
                                    return;
                                }
                            }
                            //It should not be possible to drag an item if the targetItem doesn't allow drop or source and target have different EntityManager
                            if (targetItem is EntityComponentViewModel entityTarget && !(entityTarget.Model is PrefabEntity) && entityTarget.IsDropTarget && 
                                (sourceItem.Model.EntityManager == targetItem.Model.EntityManager) && (sourceItem.Parent != targetItem))
                            {
                                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                                dropInfo.Effects = DragDropEffects.Move;
                            }
                            break;
                        default:
                            if (sourceItem.Model.Parent == targetItem.Model.Parent)
                            {
                                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                                dropInfo.Effects = DragDropEffects.Move;
                            }
                            break;
                    }
                    return;
                }


                if (dropInfo.TargetItem is EntityComponentViewModel entityComponentViewModel && !entityComponentViewModel.IsDropTarget)
                {
                    return;
                }

                //Component/Entity dragged on to another entity from Component Toolbox
                if (dropInfo.Data is ComponentToolBoxItem && dropInfo.TargetItem is EntityComponentViewModel)
                {                  
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = DragDropEffects.Copy;
                    return;
                }

                //Handle dragging of control from control repository
                if (dropInfo.Data is ControlDescriptionViewModel && dropInfo.TargetItem is EntityComponentViewModel)
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = DragDropEffects.Copy;
                    return;
                }

                //Handle dragging of application from applications repository
                if (dropInfo.Data is ApplicationDescriptionViewModel && dropInfo.TargetItem is EntityComponentViewModel)
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = DragDropEffects.Copy;
                    return;
                }

                //Handle dragging of prefab
                if (dropInfo.Data is PrefabProjectViewModel prefabProject && dropInfo.TargetItem is EntityComponentViewModel)
                {
                    if (dropInfo.VisualTarget is FrameworkElement fe && fe.DataContext.GetType() == typeof(AutomationEditorViewModel))
                    {
                        if (prefabProject.PublishedVersion.Any())
                        {
                            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                            dropInfo.Effects = DragDropEffects.Copy;
                        }
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }


        }

        public void Drop(IDropInfo dropInfo)
        {
            try
            {
               
                //Rearrange within same parent or reparent by holding Alt key
                {

                    if (dropInfo.Data is ComponentViewModel sourceItem && dropInfo.TargetItem is ComponentViewModel targetItem)
                    {
                        switch (System.Windows.Forms.Control.ModifierKeys)
                        {
                            case System.Windows.Forms.Keys.Alt:
                                if (targetItem is EntityComponentViewModel)
                                {
                                    HandleParentChange(dropInfo);
                                }
                                break;
                            default:
                                if (sourceItem.Model.Parent == targetItem.Model.Parent)
                                {
                                    HandleComponentRearrange(dropInfo);
                                }
                                break;
                        }
                        return;
                    }
                }

                if (dropInfo.TargetItem is EntityComponentViewModel entityComponentViewModel && !entityComponentViewModel.IsDropTarget)
                {
                    return;
                }

                //Adding a new component by dragging from ComponentToolBox
                {
                    if (dropInfo.Data is ComponentToolBoxItem sourceItem)
                    {
                        var automationBuilder = (dropInfo.VisualTarget as TreeView).DataContext as IEditor;
                        if (sourceItem.TypeOfComponent.GetInterface("IComponent") != null)
                        {
                            HandleComponentDrop(dropInfo);
                        }
                    }
                }

              

                if (dropInfo.Data is PrefabProjectViewModel)
                {                  
                    HandlerPrefabDrop(dropInfo);
                    return;
                }

                if (dropInfo.Data is ControlDescriptionViewModel)
                {
                    HandleControlDrop(dropInfo);
                    return;
                }

                if (dropInfo.Data is ApplicationDescriptionViewModel)
                {
                    HandleApplicationDrop(dropInfo);
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }

        }

        void HandleComponentRearrange(IDropInfo dropInfo)
        {
            if(dropInfo.Data is ComponentViewModel sourceItem && dropInfo.TargetItem is ComponentViewModel targetItem)
            {
                //Rearrange if the parents are same updating process order in the order
                if (sourceItem.Parent == targetItem.Parent)
                {
                    EntityComponentViewModel parent = targetItem.Parent;

                    int currentPosition = sourceItem.Model.ProcessOrder - 1;
                    int desiredPosition = dropInfo.InsertIndex;
                    if (desiredPosition > currentPosition)
                    {
                        desiredPosition = desiredPosition - 1;
                    }

                    if (desiredPosition == currentPosition)
                    {
                        return;
                    }
                    parent.MoveChildComponent(currentPosition, desiredPosition);
                }
            }     
            
        }      

        void HandleParentChange(IDropInfo dropInfo)
        {
            if(dropInfo.Data is ComponentViewModel sourceItem && dropInfo.TargetItem is EntityComponentViewModel targetItem)
            {
                //Allow to reparent only if entity manager is same. This will prevent TestFixture, TestCases etc to be reparented.
                if(sourceItem.Model.EntityManager == targetItem.Model.EntityManager)
                {
                    targetItem.AddComponent(sourceItem);
                }
            }           
        }

        void HandleComponentDrop(IDropInfo dropInfo)
        {                      
            if (dropInfo.Data is ComponentToolBoxItem sourceItem && dropInfo.TargetItem is EntityComponentViewModel targetItem)
            {
                //create new instance of underlying type and add to targetItem
                Type typeOfComponent = (sourceItem as ComponentToolBoxItem).TypeOfComponent;
                IComponent componentToAdd = default;

                BuilderAttribute builderAttibute = typeOfComponent.GetCustomAttributes(typeof(BuilderAttribute), false).OfType<BuilderAttribute>().FirstOrDefault();
                if (builderAttibute != null)
                {
                    IComponentBuillder componentBuilder = Activator.CreateInstance(builderAttibute.Builder) as IComponentBuillder;
                    componentToAdd = componentBuilder.CreateComponent();
                    targetItem.AddComponent(componentToAdd);
                }
                else
                {
                    componentToAdd = Activator.CreateInstance(typeOfComponent) as IComponent;
                    targetItem.AddComponent(componentToAdd);
                }

                var initializers = typeOfComponent.GetCustomAttributes(typeof(InitializerAttribute), true).OfType<InitializerAttribute>();
                foreach (var intializer in initializers)
                {
                    IComponentInitializer componentInitializer = Activator.CreateInstance(intializer.Initializer) as IComponentInitializer;
                    componentInitializer.IntializeComponent(componentToAdd, targetItem.Model.EntityManager);
                }

            }

        }

        void HandlerPrefabDrop(IDropInfo dropInfo)
        {
            if(dropInfo.Data is PrefabProjectViewModel prefabProjectViewModel && dropInfo.TargetItem is EntityComponentViewModel targetItem)
            {
                var entityManager = targetItem.Model.EntityManager;              
                var prefabDropHandler = new PrefabDropHandlerViewModel(entityManager, prefabProjectViewModel, targetItem);               
                IWindowManager windowManager = targetItem.Model.EntityManager.GetServiceOfType<IWindowManager>();
                windowManager.ShowDialogAsync(prefabDropHandler);               
            }          
        }

        void HandleControlDrop(IDropInfo dropInfo)
        {
            if (dropInfo.TargetItem is EntityComponentViewModel targetItem && dropInfo.Data is ControlDescriptionViewModel controlItem)
            {
                IControlIdentity controlIdentity = controlItem.ControlDetails;
                ContainerEntityAttribute containerEntityAttribute = controlIdentity.GetType().
                    GetCustomAttributes(typeof(ContainerEntityAttribute), false).FirstOrDefault()
                    as ContainerEntityAttribute;
                if (containerEntityAttribute != null)
                {
                    ControlEntity controlEntity = Activator.CreateInstance(containerEntityAttribute.ContainerEntityType)
                        as ControlEntity;
                    controlEntity.Name = controlItem.ControlName;
                    controlEntity.ApplicationId = controlItem.ApplicationId;
                    controlEntity.ControlId = controlItem.ControlId;
                    targetItem.AddComponent(controlEntity);
                    //Notify on event aggregator so that control references file for the project  can be updated
                    this.eventAggregator.PublishOnBackgroundThreadAsync(new ControlAddedEventArgs(controlItem.ControlDescription));
                }
            }

        }

        void HandleApplicationDrop(IDropInfo dropInfo)
        {
            if (dropInfo.TargetItem is EntityComponentViewModel targetItem && dropInfo.Data is ApplicationDescriptionViewModel applicationDescription)
            {
                var availableApplications = targetItem.Model.EntityManager.RootEntity.GetFirstComponentOfType<ApplicationPoolEntity>()?.
                                            GetComponentsOfType<ApplicationEntity>(SearchScope.Descendants) ?? Enumerable.Empty<ApplicationEntity>() ;
                bool applicationAlreadyExists = availableApplications.Any(a => a.ApplicationId.Equals(applicationDescription.ApplicationId));

                if (targetItem.Model.Tag.Equals(nameof(ApplicationPoolEntity)) && !applicationAlreadyExists)
                {                   
                    AddApplication(applicationDescription, targetItem);
                    return;
                }
                else
                {
                    if(!applicationAlreadyExists)
                    {
                        var result = MessageBox.Show("Application should be added to ApplicationPoolEntity before using it. Would you like to add it?", "Add to ApplicationPoolEntity", MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.Cancel)
                        {
                            return;
                        }
                        var rootEntity = targetItem;
                        while(rootEntity.Parent != null)
                        {
                            rootEntity = rootEntity.Parent;
                        }
                        if(rootEntity.ComponentCollection.First() is EntityComponentViewModel applicationPoolEntity)
                        {
                            AddApplication(applicationDescription, applicationPoolEntity);
                        }
                        return;
                    }
                    SequenceEntity automationSequenceEntity = new SequenceEntity()
                    {
                        Name = $"Sequence : {applicationDescription.ApplicationName}",
                        TargetAppId = applicationDescription.ApplicationId
                    };
                    targetItem.AddComponent(automationSequenceEntity);
                }              
            }           

        }

        private void AddApplication(ApplicationDescriptionViewModel applicationDescription, EntityComponentViewModel parentEntity)
        {
            ApplicationEntityAttribute applicationEntityAttribute = applicationDescription.ApplicationDetails.GetType().GetCustomAttributes(typeof(ApplicationEntityAttribute), false).FirstOrDefault()
                                                                       as ApplicationEntityAttribute;
            if (applicationEntityAttribute != null)
            {
                var applicationEntity = Activator.CreateInstance(applicationEntityAttribute.ApplicationEntity) as ApplicationEntity;
                applicationEntity.Name = $"Details : {applicationDescription.ApplicationName}";
                applicationEntity.ApplicationId = applicationDescription.ApplicationId;
                applicationEntity.EntityManager = parentEntity.Model.EntityManager;
                applicationEntity.ApplicationFile = Path.Combine(applicationSettings.ApplicationDirectory, applicationDescription.ApplicationId, $"{applicationDescription.ApplicationId}.app");

                //Add the ApplicationEntity to ApplicationPool and load ApplicationDetails
                parentEntity.AddComponent(applicationEntity);
                applicationEntity.GetTargetApplicationDetails();
            }
        }
    }
}
