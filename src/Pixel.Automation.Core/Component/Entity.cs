﻿using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core
{
    [DataContract]
    [Serializable]
    public class Entity : Component
    {
        protected List<IComponent> components = new List<IComponent>();
       
        /// <summary>
        /// List of all components added to Entity
        /// </summary>
        [DataMember(Name="Components")]
        [Browsable(false)]
        public virtual List<IComponent> Components
        {
            get
            {
                return components;
            }
            protected set
            {
                components = value;
            }
        }

        [Browsable(false)]
        public virtual ObservableCollection<IComponent> ComponentCollection
        {
            get
            {
                return new ObservableCollection<IComponent>(Components.OrderBy(c=>c.ProcessOrder));

            }
        }


        [Browsable(false)]
        /// <summary>
        /// Get all the ActorComponents added to entity in sorted order by ProcessOrder
        /// </summary>
        public IEnumerable<ActorComponent> ActorComponents
        {
            get
            {
                var actors = components.OfType<ActorComponent>();                
                return actors ?? new List<ActorComponent>();
            }
        }

        [Browsable(false)]
        public IEnumerable<Entity> Entities
        {
            get
            {
                var entities = components.OfType<Entity>();           
                return entities ?? new List<Entity>();
            }
        }


        #region Constructor
      
        public Entity() : base(string.Empty, string.Empty)
        {
          
        }

        public Entity(string name="", string tag="") : base(name:name, tag:tag)
        {
           
        }

        #endregion Constructoe  

        #region Manage Components

        /// <summary>
        /// Add a component to an entity and set its parent to entity to which it is added.
        /// No entity can contain same type of component two times.
        /// An entity can however contain multiple entity components as its child.
        /// </summary>
        /// <param name="component"></param>
        public virtual Entity AddComponent(IComponent component)
        {          
            try
            {
                if (!this.components.Contains(component))
                {                   
                    component.Parent = this;                    
                    
                    if (component.EntityManager == null)
                    {
                        component.EntityManager = this.EntityManager;
                        component.ResolveDependencies();
                        component.EntityManager.RestoreParentChildRelation(component);
                    }
                    else
                    {
                        //When adding TestCaseEntity, EntityManager is already set.
                        this.EntityManager.RestoreParentChildRelation(component);
                        component.ResolveDependencies();
                    }


                    if (this.components.Count>0)
                    {
                        component.ProcessOrder = this.components.Last().ProcessOrder + 1;
                    }
                    this.components.Add(component);

                    OnPropertyChanged(nameof(ComponentCollection));

                    component.ValidateComponent();
                        
                }
            }
            catch(Exception)
            {
                component.Parent = null;
                (component as Component).EntityManager = null;
                throw;
            }

            return this;
               
        }

        /// <summary>
        /// Remove component from this entity and set component's parent to null 
        /// </summary>
        /// <param name="component"></param>
        public virtual void RemoveComponent(IComponent component,bool dispose=true)
        {           
            if (component != null && this.components.Contains(component))
            {                
                this.components.Remove(component);
                component.Parent = null;
                component.EntityManager = null;
               
                int i = 1;
                foreach(var c in this.components)
                {
                    c.ProcessOrder = i++;
                }            

                if (dispose && component is IDisposable disposable)
                {
                    disposable.Dispose();
                }
              
                OnPropertyChanged(nameof(ComponentCollection));
               
            }
               
        }

        public void RefereshComponents()
        {
            this.Components = this.Components.OrderBy(c => c.ProcessOrder).ToList();
            OnPropertyChanged(nameof(ComponentCollection));
        }

        public virtual IEnumerable<IComponent> GetNextComponentToProcess()
        {
            foreach (var component in this.ComponentCollection)
            {
                if (!component.IsEnabled)
                {
                    continue;
                }


                if (component is IEntityProcessor)
                {
                    yield return component;
                    continue;
                }

                if (component is Entity)
                {
                    yield return component; // processors will see this as entering into a entity to call its BeforeProcess();
                    var iterator = (component as Entity).GetNextComponentToProcess().GetEnumerator();
                    while (iterator.MoveNext())
                    {
                        yield return iterator.Current;
                    }
                    yield return component; //processors will see this as exiting out of entity to call its OnCompletion();
                    continue;
                }

                //only actor components or EntityProcessors can be processed.
                //IEntityProcessor also inherit from Entity
                if (component is ActorComponent || component is AsyncActorComponent)
                {
                    yield return component;
                }
            }           
        }

        #endregion Manage Components     

        public override string ToString()
        {
            return string.Format("{{\"Id\":{0},\"Name\":{1},\"Tag\":{2},\"IsEnabled\":{3},\"Children\":{4}}}", Id, Name, Tag, IsEnabled,Components.Count);
        }
    }
}