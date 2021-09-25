﻿using Caliburn.Micro;
using GongSolutions.Wpf.DragDrop;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Control;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Components.Sequences;
using Pixel.Automation.Core.Components.TestCase;
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
        private readonly ApplicationSettings applicationSettings;

        public ComponentDropHandler(ApplicationSettings applicationSettings)
        {
            this.applicationSettings = applicationSettings;
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


                    switch (System.Windows.Forms.Control.ModifierKeys)
                    {
                        case System.Windows.Forms.Keys.Control:
                            if (targetItem is Entity)
                            {
                                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                                dropInfo.Effects = DragDropEffects.Copy;
                            }
                            break;
                        case System.Windows.Forms.Keys.Alt:
                            if (targetItem is Entity)
                            {
                                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                                dropInfo.Effects = DragDropEffects.Move;
                            }
                            break;
                        default:
                            if (sourceItem.Parent == targetItem.Parent)
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
                if (dropInfo.Data is ApplicationDescriptionViewModel && dropInfo.TargetItem is Entity)
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = DragDropEffects.Copy;
                    return;
                }

                //Handle dragging of prefab
                if (dropInfo.Data is PrefabProject prefabProject && dropInfo.TargetItem is Entity)
                {
                    if (dropInfo.VisualTarget is FrameworkElement fe && fe.DataContext.GetType() == typeof(AutomationEditorViewModel))
                    {
                        if (prefabProject.DeployedVersions.Any())
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

                if (dropInfo.Data is PrefabProject)
                {
                    var data = dropInfo.Data as PrefabProject;
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
            var sourceItem = dropInfo.Data as IComponent;
            var targetItem = dropInfo.TargetItem as IComponent;
            
            //Rearrange if the parents are same updating process order in the order
            if (sourceItem.Parent == targetItem.Parent)
            {
                Entity parentEntity = targetItem.Parent;

                int currentPosition = sourceItem.ProcessOrder - 1;
                int desiredPosition = dropInfo.InsertIndex;     
                if(desiredPosition > currentPosition)
                {
                    desiredPosition = desiredPosition - 1;
                }

                if (desiredPosition == currentPosition)
                {
                    return;
                }
                parentEntity.MoveComponent(sourceItem, currentPosition, desiredPosition);               
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
            targetItem.AddComponent(copyOfSourceItem);           
            return;
        }

        void HandleParentChange(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as Component;
            if (dropInfo.TargetItem is Entity entity)
            {
                if (sourceItem?.Parent != null)
                {
                    sourceItem.Parent.RemoveComponent(sourceItem, false);
                    sourceItem.ProcessOrder = entity.Components.Count + 1;
                    entity.AddComponent(sourceItem as IComponent);
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
                    if (builderAttibute != null)
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
                    foreach (var intializer in initializers)
                    {
                        IComponentInitializer componentInitializer = Activator.CreateInstance(intializer.Initializer) as IComponentInitializer;
                        componentInitializer.IntializeComponent(componentToAdd, parentEntity.EntityManager);
                    }

                }
            }

        }

        void HandlerPrefabDrop(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as PrefabProject;
            var targetItem = dropInfo.TargetItem as Entity;

            var prefabEntity = new PrefabEntity()
            {
                Name= sourceItem.PrefabName,
                PrefabId = sourceItem.PrefabId,
                ApplicationId = sourceItem.ApplicationId
            };


            targetItem.TryGetAnsecstorOfType<TestCaseEntity>(out TestCaseEntity testCaseEntity);
            PrefabVersionSelectorViewModel prefabVersionSelectorViewModel = new PrefabVersionSelectorViewModel(targetItem.EntityManager.GetCurrentFileSystem() as IProjectFileSystem, sourceItem, prefabEntity.Id, testCaseEntity?.Id);
            IWindowManager windowManager = targetItem.EntityManager.GetServiceOfType<IWindowManager>();
            var result = windowManager.ShowDialogAsync(prefabVersionSelectorViewModel);
            if(result.GetAwaiter().GetResult().GetValueOrDefault())
            {
                var initializers = prefabEntity.GetType().GetCustomAttributes(typeof(InitializerAttribute), true).OfType<InitializerAttribute>();
                foreach (var intializer in initializers)
                {
                    IComponentInitializer componentInitializer = Activator.CreateInstance(intializer.Initializer) as IComponentInitializer;
                    componentInitializer.IntializeComponent(prefabEntity, targetItem.EntityManager);
                }
                targetItem.AddComponent(prefabEntity);
            }
           
        }

        void HandleControlDrop(IDropInfo dropInfo)
        {
            if (dropInfo.TargetItem is Entity targetEntity && dropInfo.Data is ControlDescriptionViewModel controlItem)
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
                    controlEntity.ControlFile = Path.Combine(applicationSettings.ApplicationDirectory, controlIdentity.ApplicationId, "Controls", controlItem.ControlId, $"{controlItem.ControlId}.dat");
                    targetEntity.AddComponent(controlEntity);
                }

            }

        }

        void HandleApplicationDrop(IDropInfo dropInfo)
        {
            if (dropInfo.TargetItem is Entity targetEntity && dropInfo.Data is ApplicationDescriptionViewModel applicationDescription)
            {
                var availableApplications = targetEntity.EntityManager.RootEntity.GetFirstComponentOfType<ApplicationPoolEntity>()?.
                                            GetComponentsOfType<ApplicationEntity>(SearchScope.Descendants) ?? Enumerable.Empty<ApplicationEntity>() ;
                bool applicationAlreadyExists = availableApplications.Any(a => a.ApplicationId.Equals(applicationDescription.ApplicationId));

                if (targetEntity.Tag.Equals(nameof(ApplicationPoolEntity)) && !applicationAlreadyExists)
                {                   
                    AddApplication(applicationDescription, targetEntity);
                    return;
                }
                else
                {
                    if(!applicationAlreadyExists)
                    {
                        var result = MessageBox.Show("Application should be added to ApplicationPoolEntity before using it. Would you like to add it?", "Add to ApplicationPoolEntity", MessageBoxButton.OKCancel);
                        if(result == MessageBoxResult.Cancel)
                        {
                            return;                           
                        }
                        var applicationPoolEntity = targetEntity.EntityManager.RootEntity.GetFirstComponentOfType<ApplicationPoolEntity>();
                        AddApplication(applicationDescription, applicationPoolEntity);
                    }
                    SequenceEntity automationSequenceEntity = new SequenceEntity()
                    {
                        Name = $"Sequence : {applicationDescription.ApplicationName}",
                        TargetAppId = applicationDescription.ApplicationId
                    };
                    targetEntity.AddComponent(automationSequenceEntity);
                }              
            }           

        }

        private void AddApplication(ApplicationDescriptionViewModel applicationDescription, Entity parentEntity)
        {
            ApplicationEntityAttribute applicationEntityAttribute = applicationDescription.ApplicationDetails.GetType().GetCustomAttributes(typeof(ApplicationEntityAttribute), false).FirstOrDefault()
                                                                       as ApplicationEntityAttribute;
            if (applicationEntityAttribute != null)
            {
                var applicationEntity = Activator.CreateInstance(applicationEntityAttribute.ApplicationEntity) as ApplicationEntity;
                applicationEntity.Name = $"Details : {applicationDescription.ApplicationName}";
                applicationEntity.ApplicationId = applicationDescription.ApplicationId;
                applicationEntity.EntityManager = parentEntity.EntityManager;
                applicationEntity.ApplicationFile = Path.Combine(applicationSettings.ApplicationDirectory, applicationDescription.ApplicationId, $"{applicationDescription.ApplicationId}.app");

                //Add the ApplicationEntity to ApplicationPool and load ApplicationDetails
                parentEntity.AddComponent(applicationEntity);
                applicationEntity.GetTargetApplicationDetails();
            }
        }
    }
}
