using GongSolutions.Wpf.DragDrop;
using Pixel.Automation.AppExplorer.ViewModels.Control;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Components.Sequences;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Designer.ViewModels.DragDropHandlers
{
    public class ComponentDropHandler : IDropTarget
    {
        private readonly ILogger logger = Log.ForContext<ComponentDropHandler>();

        public void DragOver(IDropInfo dropInfo)
        {
            try
            {
                //if source && target are same , do nothing
                if (dropInfo.Data == dropInfo.TargetItem)
                {
                    return;
                }

                //Component/Entity dragged on to another entity from Component Toolbox
                if (dropInfo.Data is ComponentToolBoxItem && dropInfo.TargetItem is Entity)
                {
                    //don't allow applications to be dropped on process canvas
                    if ((dropInfo.Data as ComponentToolBoxItem).TypeOfComponent.GetInterface("IApplication") != null)
                    {
                        return;
                    }
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = DragDropEffects.Copy;
                    return;
                }

                //Component/Entity is being rearranged i.e. dragged away from its parent Entity to some other Entity
                if (dropInfo.Data is IComponent && dropInfo.TargetItem is IComponent)
                {
                    IComponent sourceItem = dropInfo.Data as IComponent;
                    IComponent targetItem = dropInfo.TargetItem as IComponent;


                    switch(System.Windows.Forms.Control.ModifierKeys)
                    {
                        case System.Windows.Forms.Keys.Control:
                            if (targetItem is Entity)
                            {
                                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                                dropInfo.Effects = DragDropEffects.Copy;
                            }                          
                            break;
                        case System.Windows.Forms.Keys.Alt:
                            if(targetItem is Entity)
                            {
                                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                                dropInfo.Effects = DragDropEffects.Move;
                            }
                            break;
                        default:
                            if(sourceItem.Parent==targetItem.Parent)
                            {
                                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                                dropInfo.Effects = DragDropEffects.Move;
                            }                                              
                            break;
                    }
                    return;                 
                }


                //Handle dragging of control from control repository
                if (dropInfo.Data is ControlDescriptionViewModel && dropInfo.TargetItem is Entity)
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = DragDropEffects.Copy;
                    return;
                }

                //Handle dragging of application from applications repository
                if (dropInfo.Data is ApplicationDescription && dropInfo.TargetItem is Entity)
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = DragDropEffects.Copy;
                    return;
                }

                //Handle dragging of prefab
                if (dropInfo.Data is PrefabDescription prefabDescription && dropInfo.TargetItem is Entity)
                {
                    if (prefabDescription.DeployedVersions.Any())
                    {
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                        dropInfo.Effects = DragDropEffects.Copy;
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
                //Adding a new component by dragging from ComponentToolBox
                if (dropInfo.Data is ComponentToolBoxItem)
                {
                    var sourceItem = dropInfo.Data as ComponentToolBoxItem;
                    var automationBuilder = (dropInfo.VisualTarget as TreeView).DataContext as IEditor;                 
                                    
                    if (sourceItem.TypeOfComponent.GetInterface("IComponent") != null)
                    {
                        HandleComponentDrop(dropInfo);
                    }
                }

                if (dropInfo.Data is IComponent && dropInfo.TargetItem is IComponent)
                {
                    IComponent sourceItem = dropInfo.Data as IComponent;
                    IComponent targetItem = dropInfo.TargetItem as IComponent;

                    switch (System.Windows.Forms.Control.ModifierKeys)
                    {
                        case System.Windows.Forms.Keys.Control:
                            if (targetItem is Entity)
                            {
                                HandleDuplicateComponent(dropInfo);
                            }
                            break;
                        case System.Windows.Forms.Keys.Alt:
                            if (targetItem is Entity)
                            {
                                HandleParentChange(dropInfo);
                            }
                            break;
                        default:
                            if (sourceItem.Parent == targetItem.Parent)
                            {
                                HandleComponentRearrange(dropInfo);
                            }
                            break;
                    }
                    return;                
                }

                if (dropInfo.Data is PrefabDescription)
                {
                    var data = dropInfo.Data as PrefabDescription;                 
                    HandlerPrefabDrop(dropInfo);
                    return;
                }

                if (dropInfo.Data is ControlDescriptionViewModel)
                {
                    HandleControlDrop(dropInfo);
                    return;
                }

                if (dropInfo.Data is ApplicationDescription)
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
            var sourceItem = dropInfo.Data as IComponent;
            var targetItem = dropInfo.TargetItem as IComponent;
           
     
            //Rearrange if the parents are same updating process order in the order
            if(sourceItem.Parent==targetItem.Parent)
            {
                Entity parentEntity = targetItem.Parent;

                int desiredPosition = dropInfo.InsertIndex;
                if(desiredPosition>parentEntity.Components.Count)
                {
                    desiredPosition = desiredPosition - 1;
                }
                int currentPosition = sourceItem.ProcessOrder-1;

                if(desiredPosition==currentPosition)
                {
                    return;
                }
                else if (desiredPosition>currentPosition)
                {              
                    while(desiredPosition > currentPosition)
                    {
                        parentEntity.Components.ElementAt(desiredPosition-1).ProcessOrder--;
                        desiredPosition--;
                    }
                    sourceItem.ProcessOrder = dropInfo.InsertIndex>parentEntity.Components.Count? dropInfo.InsertIndex-1 : dropInfo.InsertIndex;
                    parentEntity.RefereshComponents();
                   return;
                }
                else
                {               
                    while(desiredPosition < currentPosition)
                    {
                        parentEntity.Components.ElementAt(desiredPosition).ProcessOrder++;
                        desiredPosition++;
                    }
                    sourceItem.ProcessOrder = dropInfo.InsertIndex+1;
                    parentEntity.RefereshComponents();                   
                    return;
                }            
            }
        }

        void HandleDuplicateComponent(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as IComponent;
            var targetItem = dropInfo.TargetItem as Entity;          
          
            IComponent copyOfSourceItem;
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, sourceItem);
                stream.Seek(0, SeekOrigin.Begin);
                copyOfSourceItem = (IComponent)formatter.Deserialize(stream);
            }

            //Entity with same name can exist inside same parent. However, components can't.
            //if(!(sourceItem is Entity))
            //{
            //    int copyNumber = 1;
            //    string copyName = string.Empty;
            //    while(true)
            //    {
            //        copyName = $"{copyOfSourceItem.Name.Split(new char[] {'-'})[0]}-{copyNumber}";
            //        if(targetItem.Components.Any(a=>a.Name.Equals(copyName)))
            //        {
            //            copyNumber++;
            //            continue;
            //        }
            //        break;
            //    }
            //    copyOfSourceItem.Name = copyName;
            //}

            targetItem.AddComponent(copyOfSourceItem);
            targetItem.RefereshComponents();
            return;
        }

        void HandleParentChange(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as IComponent;
            var targetItem = dropInfo.TargetItem as IComponent;

            //if (targetItem is AutomationUnitEntity && sourceItem is AutomationUnitEntity)
            //    return;

            if (targetItem is Entity)
            {
                if (sourceItem is Component && (sourceItem as Component).Parent != null)
                {
                    (sourceItem as Component).Parent.RemoveComponent(sourceItem as IComponent, false);
                    (sourceItem as Component).ProcessOrder = (targetItem as Entity).Components.Count + 1;
                    (targetItem as Entity).AddComponent(sourceItem as IComponent);
                }
            }
        }

        void HandleComponentDrop(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as ComponentToolBoxItem;
            var targetItem = dropInfo.TargetItem as IComponent;

            if (targetItem is Entity parentEntity)
            {
                if (sourceItem is ComponentToolBoxItem)
                {
                    //create new instance of underlying type and add to targetItem
                    Type typeOfComponent = (sourceItem as ComponentToolBoxItem).TypeOfComponent;
                    IComponent componentToAdd = default;

                    BuilderAttribute builderAttibute = typeOfComponent.GetCustomAttributes(typeof(BuilderAttribute), false).OfType<BuilderAttribute>().FirstOrDefault();
                    if(builderAttibute != null)
                    {
                        IComponentBuillder componentBuilder = Activator.CreateInstance(builderAttibute.Builder) as IComponentBuillder;
                        componentToAdd = componentBuilder.CreateComponent();
                        parentEntity.AddComponent(componentToAdd);                      
                    }
                    else
                    {
                        componentToAdd = Activator.CreateInstance(typeOfComponent) as IComponent;
                        parentEntity.AddComponent(componentToAdd);
                    }

                    var initializers = typeOfComponent.GetCustomAttributes(typeof(InitializerAttribute), true).OfType<InitializerAttribute>();
                    foreach(var intializer in initializers)
                    {
                        IComponentInitializer componentInitializer = Activator.CreateInstance(intializer.Initializer) as IComponentInitializer;
                        componentInitializer.IntializeComponent(componentToAdd, parentEntity.EntityManager);
                    }
                  
                }
            }

        }       

        void HandlerPrefabDrop(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as PrefabDescription;           
            var targetItem = dropInfo.TargetItem as Entity;

            var prefabEntity = new PrefabEntity()
            {
                PrefabId = sourceItem.PrefabId,
                ApplicationId = sourceItem.ApplicationId,
                PrefabVersion = sourceItem.DeployedVersions.OrderBy(a => a.Version).Last()
            };

            var initializers = prefabEntity.GetType().GetCustomAttributes(typeof(InitializerAttribute), true).OfType<InitializerAttribute>();
            foreach (var intializer in initializers)
            {
                IComponentInitializer componentInitializer = Activator.CreateInstance(intializer.Initializer) as IComponentInitializer;
                componentInitializer.IntializeComponent(prefabEntity, targetItem.EntityManager);
            }

            targetItem.AddComponent(prefabEntity);          
        }

        void HandleControlDrop(IDropInfo dropInfo)
        {                 
            if (dropInfo.TargetItem is Entity targetEntity && dropInfo.Data is ControlDescriptionViewModel controlItem)
            {              
                IControlIdentity controlIdentity = controlItem.ControlDetails;
                ContainerEntityAttribute containerEntityAttribute = controlIdentity.GetType().
                    GetCustomAttributes(typeof(ContainerEntityAttribute), false).FirstOrDefault() 
                    as ContainerEntityAttribute;
                if(containerEntityAttribute != null)
                {
                    ControlEntity controlEntity = Activator.CreateInstance(containerEntityAttribute.ContainerEntityType)
                        as ControlEntity;
                    controlEntity.Name = controlItem.ControlName;
                    controlEntity.ControlFile = Path.Combine("ApplicationsRepository", controlIdentity.ApplicationId, "Controls", controlItem.ControlId, $"{controlItem.ControlId}.dat");
                    targetEntity.AddComponent(controlEntity);
                }
                //else
                //{
                //    ControlEntity controlEntity = new ControlEntity()
                //    {
                //        Name = controlItem.Name,
                //        ControlDetails = controlItem.ControlDetails
                //    };
                //    targetItem.AddComponent(controlEntity);
                //}                
               
                         
            }

        }


        void HandleApplicationDrop(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as ApplicationDescription;
            var targetItem = dropInfo.TargetItem;

            if (targetItem is Entity)
            {
                Entity targetEntity = targetItem as Entity;
                var availableApplications = targetEntity.EntityManager.RootEntity.GetFirstComponentOfType<ApplicationPoolEntity>().GetComponentsOfType<ApplicationEntity>(SearchScope.Descendants);
                if (targetEntity.Tag.Equals("ApplicationPoolEntity"))
                {
                    if (availableApplications != null)
                    {
                        if (availableApplications.Any(a => a.ApplicationId.Equals(sourceItem.ApplicationId)))
                        {
                            logger.Information("An attempt was made to add application with id : {applicationId} to the Application Pool. Application with such id already exists.", sourceItem.ApplicationId);
                            return;

                        }
                    }
                    //Add the application to the Application Pool
                    ApplicationEntity appDetailsEntity = new ApplicationEntity()
                    {
                        Name = $"Details : {sourceItem.ApplicationName}",
                        ApplicationId = sourceItem.ApplicationId,
                        EntityManager = targetEntity.EntityManager,
                        ApplicationFile = Path.Combine("ApplicationsRepository", sourceItem.ApplicationId, $"{sourceItem.ApplicationId}.app")
                    
                    };
                    appDetailsEntity.GetTargetApplicationDetails();
                    targetEntity.AddComponent(appDetailsEntity);
                    return;
                }
                else
                {
                    //Create and add a AutomationSequnceEntity with its TargetAppId and Name appropriately initialized                    
                    if (availableApplications != null)
                    {
                        bool applicationExists = false;
                        foreach (ApplicationEntity app in availableApplications)
                        {
                            if (app.ApplicationId.ToString().Equals(sourceItem.ApplicationId))
                            {
                                SequenceEntity automationSequenceEntity = new SequenceEntity()
                                {
                                    Name = $"Sequence : {sourceItem.ApplicationName}",
                                    TargetAppId = app.ApplicationId
                                };
                                targetEntity.AddComponent(automationSequenceEntity);
                                applicationExists = true;
                                break;
                            }
                        }
                        if (!applicationExists)
                        {
                            //TODO : Show message box to the user and with his consent automatically add the application to the Pool
                            logger.Warning($"Automation Sequence can't be created for application : {sourceItem.ApplicationName}. Please add this application to Application Pool first");
                        }
                    }
                }
            }

        }
    }
}
