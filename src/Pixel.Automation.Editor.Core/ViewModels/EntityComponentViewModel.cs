using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components.Processors;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Pixel.Automation.Editor.Core.ViewModels
{
    public class EntityComponentViewModel : ComponentViewModel
    {
        public bool IsDropTarget { get; protected set; } = true;

        public ObservableCollection<ComponentViewModel> ComponentCollection { get; private set; } = new();

        public EntityComponentViewModel(Entity model)
        {
            Model = Guard.Argument(model).NotNull().Value;
        }

        public EntityComponentViewModel(Entity model, EntityComponentViewModel parent) : base(model, parent)
        {
            if (model.GetType().GetCustomAttributes(true).Any(a => a is NoDropTargetAttribute))
            {
                this.IsDropTarget = false;
            }
        }

        public void AddComponent(ComponentViewModel component)
        {
            Guard.Argument(component).NotNull();
            if (component.Model.EntityManager != null && component.Model.EntityManager != this.Model.EntityManager)
            {
                throw new InvalidOperationException("Reparenting is allowed only when EntityManager are same");
            }
            if (component.Parent != null)
            {
                component.Parent.RemoveComponent(component);
            }          
            var entityModel = this.Model as Entity;
            entityModel.AddComponent(component.Model);
            //some entities e.g. GroupEntity, loops, etc don't allow component to be added to them and simply  return itself without 
            //actually doing anything.We check that component was actually added as a child before we added view model as child.
            if (entityModel.Components.Contains(component.Model))
            {
                component.Parent = this;
                this.ComponentCollection.Add(component);
            }
        }

        public void RemoveComponent(ComponentViewModel componentToDelete)
        {
            if (this.ComponentCollection.Contains(componentToDelete))
            {
                (this.Model as Entity).RemoveComponent(componentToDelete.Model);
                componentToDelete.Parent = null;
                this.ComponentCollection.Remove(componentToDelete);               
            }
        }

        public void AddComponent(IComponent componentToAdd)
        {
            var model = this.Model as Entity;
            model.AddComponent(componentToAdd);
            //some entities e.g. GroupEntity, loops, etc don't allow component to be added to them and simply  return itself without 
            //actually doing anything.We check that component was actually added as a child before we added view model as child.
            if (model.Components.Contains(componentToAdd))
            {
                if (componentToAdd is Entity entity)
                {
                    var root = GetViewModelForComponent(entity, this);
                    this.ComponentCollection.Add(root);
                    if (entity.Components.Any())
                    {
                        Queue<ComponentViewModel> componentViewModels = new();
                        componentViewModels.Enqueue(root);
                        while (componentViewModels.TryDequeue(out ComponentViewModel componentViewModel) 
                            && componentViewModel is EntityComponentViewModel current)
                        {
                            var entityModel = current.Model as Entity;
                            foreach (var component in entityModel.Components)
                            {                             
                                if (component is Entity e)
                                {
                                    var evm = GetViewModelForComponent(e, current);
                                    current.AddComponent(evm);
                                    componentViewModels.Enqueue(evm);
                                    continue;
                                }                                
                                current.AddComponent(GetViewModelForComponent(component, current));
                            }
                        }
                    }                    
                }
                else
                {
                    this.ComponentCollection.Add(GetViewModelForComponent(componentToAdd, this));
                }
            }           
        }

        protected ComponentViewModel GetViewModelForComponent(IComponent component, EntityComponentViewModel parent)
        {
            if(component is EntityProcessor entityProcessor)
            {
                return new EntityProcessorViewModel(entityProcessor, parent);
            }
            if(component is Entity entity)
            {
                return new EntityComponentViewModel(entity, parent);
            }
            return new ComponentViewModel(component, parent);
        }


        public void RemoveComponent(IComponent componentToDelete)
        {
            for (int i = 0; i < this.ComponentCollection.Count; i++)
            {
                if (this.ComponentCollection[i].Model.Equals(componentToDelete))
                {
                    RemoveComponent(this.ComponentCollection[i]);
                    break;
                }
            }
        }

        public void MoveChildComponent(int oldIndex, int newIndex)
        {
            if (this.ComponentCollection.Count > oldIndex && this.ComponentCollection.Count > newIndex)
            {
                (Model as Entity).MoveComponent(oldIndex, newIndex);
                this.ComponentCollection.Move(oldIndex, newIndex);
                NotifyOfPropertyChange(nameof(ProcessOrder));
            }
        }
    }
}
