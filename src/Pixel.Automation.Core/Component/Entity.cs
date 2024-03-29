﻿using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core
{
    /// <summary>
    /// Entity is a <see cref="IComponent"/> which can have a collection of <see cref="IComponent"/> as it's child
    /// </summary>
    [DataContract]
    [Serializable]
    public class Entity : Component
    {
        protected readonly List<IComponent> components = new ();       
        /// <summary>
        /// List of all components added to Entity
        /// </summary>
        [DataMember(Order = 100)]
        [Browsable(false)]
        public virtual List<IComponent> Components
        {
            get
            {
                return components;
            }           
        }
       
        /// <summary>
        /// Get all child components of type Entity
        /// </summary>
        [Browsable(false)]
        public IEnumerable<Entity> Entities => components.OfType<Entity>() ?? Enumerable.Empty<Entity>();

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Entity() : base(string.Empty, string.Empty)
        {
          
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the entity</param>
        /// <param name="tag">Tag assigned to entity</param>
        public Entity(string name = "", string tag = "") : base(name:name, tag:tag)
        {
           
        }

        #endregion Constructoe  

        #region before proces, after process and on fault hooks

        /// <summary>
        /// Processor will call BeforeProcess on an Entity before processing it's child components
        /// </summary>
        public virtual async Task BeforeProcessAsync()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Processor will call OnCompletionAsync on an Entity after processing it's child components
        /// </summary>
        public virtual async Task OnCompletionAsync()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Processor will call OnFaultAsync on an Entity if one of it's child components runs in to an exception during execution
        /// </summary>
        public virtual async Task OnFaultAsync(IComponent faultingComponent)
        {
            await Task.CompletedTask;
        }

        #endregion before proces, after process and on fault hooks

        #region Manage Components

        /// <summary>
        /// Add a component to an entity and set its parent to entity to which it is added.      
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
                        if(component is Entity entity)
                        {
                            component.EntityManager.RestoreParentChildRelation(entity);
                        }
                    }
                    else
                    {
                        //TestCase and TestFixture Entity have their own non-primary EntityManager
                        if (component is Entity entity)
                        {
                            this.EntityManager.RestoreParentChildRelation(entity);
                        }                       
                        component.ResolveDependencies();
                    }

                    if (this.components.Count>0)
                    {
                        component.ProcessOrder = this.components.Last().ProcessOrder + 1;
                    }
                  
                    this.Components.Add(component);                  
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
        public virtual void RemoveComponent(IComponent component, bool dispose = true)
        {           
            if (component != null && this.components.Contains(component))
            {                
                this.components.Remove(component);               
                component.Parent = null;               
               
                int i = 1;
                foreach(var c in this.components)
                {
                    c.ProcessOrder = i++;
                }            

                if (dispose && component is IDisposable disposable)
                {
                    component.EntityManager = null;
                    disposable.Dispose();
                }               
            }
               
        }

        /// <summary>
        /// Move an existing component to a new position.
        /// This process will update the ProcessOrder for all siblings.
        /// </summary>
        /// <param name="target">Component to be moved to a new index</param>
        /// <param name="oldIndex">Old index of the component</param>
        /// <param name="newIndex">New index of the component</param>
        public void MoveComponent(int oldIndex, int newIndex)
        {
            var componentToMove = this.Components.ElementAt(oldIndex);
            this.Components.RemoveAt(oldIndex);
            this.Components.Insert(newIndex, componentToMove);
            int i = 1;
            foreach (var c in this.components)
            {
                c.ProcessOrder = i++;
            }
        }

        /// <summary>
        /// Get the next component (breadth - first) to process based on ProcessOrder of the components.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<IComponent> GetNextComponentToProcess()
        {
            foreach (var component in this.components)
            {
                if (!component.IsEnabled)
                {
                    continue;
                }

                //EntityProcessors are Entity that also implement IEntityProcessor. We don't want to process
                //it's child components and hence treat it differently.
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
                if (component is ActorComponent)
                {
                    yield return component;
                }
            }           
        }

        #endregion Manage Components     

        public override string ToString()
        {
            return string.Format("{{\"Id\":{0},\"Name\":{1},\"Tag\":{2},\"IsEnabled\":{3},\"Children\":{4}}}", Id, Name, Tag, IsEnabled, Components.Count);
        }
    }
}
