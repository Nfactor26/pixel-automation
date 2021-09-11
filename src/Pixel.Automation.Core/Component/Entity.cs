using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
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

        protected ObservableCollection<IComponent> componentCollection;
        /// <summary>
        /// This is bound in view
        /// </summary>
        [Browsable(false)]
        public virtual ObservableCollection<IComponent> ComponentCollection
        {
            get
            {
                if(componentCollection == null)
                {
                    componentCollection = new ObservableCollection<IComponent>(this.Components);
                }
                return componentCollection;
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
                  
                    this.Components.Add(component);
                    if(!this.ComponentCollection.Contains(component))
                    {
                        this.ComponentCollection.Add(component);
                    }
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
                this.Components.Remove(component);
                if(this.ComponentCollection.Contains(component))
                {
                    this.ComponentCollection.Remove(component);
                }

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
            }
               
        }

        /// <summary>
        /// Move an existing component to a new position.
        /// This process will update the ProcessOrder for all siblings.
        /// </summary>
        /// <param name="target">Component to be moved to a new index</param>
        /// <param name="oldIndex">Old index of the component</param>
        /// <param name="newIndex">New index of the component</param>
        public void MoveComponent(IComponent target, int oldIndex, int newIndex)
        {
            //int count = this.ComponentCollection.Count;
            if(this.ComponentCollection.Contains(target))
            {
                this.ComponentCollection.Move(oldIndex, newIndex);
                int processOrder = 1;
                foreach (var component in this.ComponentCollection)
                {
                    component.ProcessOrder = processOrder;
                    processOrder++;
                }

                this.Components = this.Components.OrderBy(c => c.ProcessOrder).ToList();
            }
        }

        /// <summary>
        /// Get the next component (breadth - first) to process based on ProcessOrder of the components.
        /// </summary>
        /// <returns></returns>
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
