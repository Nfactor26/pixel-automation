using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixel.Automation.Core
{
    public static class EntityExtensions
    {
        /// <summary>
        /// Returns all descendant components of rootEntity
        /// </summary>
        /// <param name="rootEntity"></param>
        /// <returns></returns>
        public static List<IComponent> GetAllComponents(this Entity rootEntity)
        {
            List<IComponent> childComponents = new List<IComponent>();
            foreach (var component in rootEntity.Components)
            {                
                childComponents.Add(component);                
                if (component is Entity)
                {
                    childComponents.AddRange(GetAllComponents(component as Entity));
                }
            }
            return childComponents;
        }

        /// <summary>
        /// Check if a component of given type exists within specified SearchScope of the Entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rootEntity"></param>
        /// <param name="searchScope"></param>
        /// <returns></returns>
        public static bool HasComponentsOfType<T>(this Entity rootEntity, SearchScope searchScope = SearchScope.Descendants)
        {
            switch (searchScope)
            {
                case SearchScope.Children:
                    return rootEntity.Components.OfType<T>().Any();                  
                case SearchScope.Descendants:
                    List<IComponent> childComponents = GetAllComponents(rootEntity);                   
                    foreach (var component in childComponents)
                    {
                        if (component is T)
                        {
                            return true;
                        }
                    }
                    break;              
            }
            return false;
        }

        /// <summary>
        /// Returns all components of Type : T in the rootEntity or any of its children
        /// </summary>
        /// <param name="rootEntity"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetComponentsOfType<T>(this Entity rootEntity,SearchScope searchScope=SearchScope.Children)
        {
            switch(searchScope)
            {
                case SearchScope.Children:
                    IEnumerable<T> components = rootEntity.Components.OfType<T>();                  
                    return components;

                case SearchScope.Descendants:
                    List<IComponent> childComponents = GetAllComponents(rootEntity);
                    List<T> componentsOfTypeT = new List<T>();
                    foreach (var component in childComponents)
                    {
                        if (component is T)
                        {
                            componentsOfTypeT.Add((T)component);
                        }
                    }
                    return componentsOfTypeT;

                default:
                    throw new ArgumentException($"SearchScope - {searchScope} is not supported by operation {nameof(GetComponentsOfType)}");
            }           
        }

        /// <summary>
        /// Locate the first component of specified type inside rootEntity.
        /// </summary>
        /// <typeparam name="T">Required type T of component to be located</typeparam>
        /// <param name="rootEntity">Entity within which component needs to be looked for</param>
        /// <exception cref="MissingComponentException">thrown if no component of specified type exists in rootEntity</exception>
        /// <returns>First occurence of component of type T inside children of rootEntity</returns>
        public static T GetFirstComponentOfType<T>(this Entity rootEntity, SearchScope searchScope = SearchScope.Children, bool throwIfMissing = true)
        {
            switch (searchScope)
            {
                case SearchScope.Children:
                    foreach (var p in rootEntity.Components)
                    {
                        if (p is T)
                        {
                            return (T)p;
                        }
                    }
                    break;
                case SearchScope.Descendants:
                    foreach (var component in rootEntity.Components)
                    {
                        if (component is T)
                        {
                            return (T)component;
                        }
                        if (component is Entity)
                        {
                            var foundComponent = (component as Entity).GetFirstComponentOfType<T>(searchScope, false);  
                            if(foundComponent != null)
                            {
                                return foundComponent;
                            }
                        }
                    }
                    break;
                default:
                    throw new ArgumentException($"SearchScope - {searchScope} is not supported by operation GetComponentsOftype<>");
            }       

            if(throwIfMissing)
            {
                throw new MissingComponentException($"No component of required type {typeof(T).ToString()} exists inside component : {rootEntity.ToString()}");
            }

            return default;
               
        }

        /// <summary>
        /// Returns all components with tag : tag in the rootEntity or any of its children
        /// </summary>
        /// <param name="rootEntity"></param>
        /// <returns></returns>
        public static IEnumerable<IComponent> GetComponentsByTag(this Entity rootEntity,string tag, SearchScope searchScope = SearchScope.Children)
        {
            switch(searchScope)
            {
                case SearchScope.Children:
                    var components = rootEntity.Components.Where(c => c.Tag.Equals(tag));                   
                    return components;

                case SearchScope.Descendants:
                    List<IComponent> childComponents = GetAllComponents(rootEntity);
                    List<IComponent> componentsWithTag = new List<IComponent>();
                    foreach (var component in childComponents)
                    {
                        if (component.Tag.Equals(tag))
                        {
                            componentsWithTag.Add(component);
                        }
                    }
                    return componentsWithTag;
                default:
                    throw new ArgumentException($"SearchScope - {searchScope} is not supported by operation {nameof(GetComponentsByTag)}");
            }           
        }

        /// <summary>
        /// Returns the component with id : id .
        /// </summary>
        /// <param name="rootEntity"></param>
        /// <param name="id"></param>
        /// <param name="searchScope">Whether to look in immediate children or entire subtree</param>
        /// <returns></returns>
        public static IComponent GetComponentById(this Entity rootEntity, string id, SearchScope searchScope = SearchScope.Children)
        {
            IComponent foundComponent = null;
            switch (searchScope)
            {
                case SearchScope.Children:
                    foundComponent = rootEntity.Components.FirstOrDefault(c => c.Id.Equals(id));
                    break;
                                 
                case SearchScope.Descendants:
                    foreach (var component in rootEntity.Components)
                    {
                        if (component.Id.Equals(id))
                        {
                            return component;
                        }
                       
                        if (component is Entity)
                        {
                            foundComponent = (component as Entity).GetComponentById(id, searchScope);
                            if (foundComponent != null)
                            {
                                break;
                            }
                        }
                    }
                    break;
                default:
                    throw new ArgumentException($"SearchScope - {searchScope} is not supported by operation {nameof(GetComponentById)}");
            }
         

            return foundComponent;
        }

        /// <summary>
        /// Returns the component with Name : name
        /// </summary>
        /// <param name="rootEntity"></param>
        /// <param name="name">Name of the component to find</param>
        /// <param name="searchScope">Whether to look in immediate children or entire subtree</param>
        /// <returns></returns>
        public static IEnumerable<IComponent> GetComponentsByName(this Entity rootEntity, string name, SearchScope searchScope = SearchScope.Children)
        {

            switch (searchScope)
            {
                case SearchScope.Children:
                    var components = rootEntity.Components.Where(c => c.Name.Equals(name));                
                    return components;

                case SearchScope.Descendants:
                    List<IComponent> childComponents = GetAllComponents(rootEntity);
                    List<IComponent> componentsWithName = new List<IComponent>();
                    foreach (var component in childComponents)
                    {
                        if (component.Name.Equals(name))
                        {
                            componentsWithName.Add(component);
                        }

                    }
                    return componentsWithName;

                default:
                    throw new ArgumentException($"SearchScope - {searchScope} is not supported by operation {nameof(GetComponentsByName)}");
            }
           
        }

        /// <summary>
        /// Get components with specified attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rootEntity">Parent entity</param>
        /// <param name="searchScope">SearchScope for component to be looked</param>
        /// <returns></returns>
        public static IEnumerable<IComponent> GetComponentsWithAttribute<T>(this Entity rootEntity, SearchScope searchScope = SearchScope.Children)
        {
            IEnumerable<IComponent> components = default;
            switch (searchScope)
            {
                case SearchScope.Children:
                    components = rootEntity.Components.Where(c => c.GetType().GetCustomAttributes(true).Any(a => a is T));                 
                    return components ?? Enumerable.Empty<IComponent>(); 

                case SearchScope.Descendants:
                    components = GetAllComponents(rootEntity);
                    components = components.Where(c => c.GetType().GetCustomAttributes(true).Any(a => a is T));
                    return components ?? Enumerable.Empty<IComponent>();
               
                default:
                    throw new ArgumentException($"SearchScope - {searchScope} is not supported by operation {nameof(GetComponentsWithAttribute)}");
            }
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentComponent"></param>
        /// <returns></returns>
        public static Entity GetRootEntity(this IComponent currentComponent)
        {
            IComponent current = currentComponent;
            while(current.Parent != null)
            {
                current = current.Parent;
            }
            return current as Entity;
        }

        /// <summary>
        /// Reseset all the components in hierarchy starting from rootEntity in depth first order
        /// </summary>
        /// <param name="rootEntity">Entity whose hierarchy neeeds to be reset</param>
        public static void ResetHierarchy(this Entity rootEntity)
        {
            foreach(var component in rootEntity.Components)
            {
                if(component is Entity)
                {
                    (component as Entity).ResetHierarchy();
                    continue;
                }
                component.ResetComponent();
            }
            rootEntity.ResetComponent();
        }
       
        /// <summary>
        /// Get all child components that implement ILoop
        /// </summary>
        /// <param name="rootEntity"></param>
        /// <returns></returns>
        public static IEnumerable<ILoop> GetInnerLoops(this Entity rootEntity)
        {
            foreach (var component in rootEntity.Components)
            {
                if (!component.IsEnabled)
                {
                    continue;
                }

                if (component is ILoop loopComponent)
                {
                    yield return loopComponent;
                    continue;
                }

                if (component is Entity entity)
                {
                    foreach (var item in GetInnerLoops(entity))
                    {
                        yield return item;
                    }
                }
            }
        }

        /// <summary>
        /// Find ancestor component of a given type for a specified component
        /// </summary>
        /// <typeparam name="T">Type of ancestor</typeparam>
        /// <param name="component"></param>
        /// <returns></returns>
        /// <exception cref="MissingComponentException">Thrown when ancestor of specified  type T could not be found</exception>
        public static T GetAnsecstorOfType<T>(this IComponent component) where T : class, IComponent
        {
            var parent = component.Parent;
            while(parent!=null)
            {               
                if (parent is T requiredComponent)
                {
                    return requiredComponent;
                }                   
                parent = parent.Parent;
            }
            throw new MissingComponentException($"Ancestor of type :{typeof(T)} could not be located");
        }
      
        /// <summary>
        /// Try to get an ancestor component of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <param name="ancestor"></param>
        /// <returns>True if ancestor could be located</returns>
        public static bool TryGetAnsecstorOfType<T>(this IComponent component, out T ancestor) where T : class, IComponent
        {
            var parent = component.Parent;
            while (parent != null)
            {
                if (parent is T requiredComponent)
                {
                    ancestor = requiredComponent;
                    return true;
                }
                parent = parent.Parent;
            }
            ancestor = null;
            return false;
        }
    }
   
}
