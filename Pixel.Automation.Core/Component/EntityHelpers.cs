using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    if (components == null)
                        components = new List<T>();
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
            }

            return null;
           
        }

        /// <summary>
        /// Locate the first component of specified type inside rootEntity.
        /// </summary>
        /// <typeparam name="T">Required type T of component to be located</typeparam>
        /// <param name="rootEntity">Entity within which component needs to be looked for</param>
        /// <exception cref="MissingComponentException">thrown if no component of specified type exists in rootEntity</exception>
        /// <returns>First occurence of component of type T inside children of rootEntity</returns>
        public static T GetFirstComponentOfType<T>(this Entity rootEntity, SearchScope searchScope = SearchScope.Children)
        {
            switch (searchScope)
            {
                case SearchScope.Children:
                    foreach (var p in rootEntity.Components)
                    {
                        if (p is T)
                            return (T)p;
                    }
                    break;
                case SearchScope.Descendants:
                    foreach (var component in rootEntity.Components)
                    {
                        if (component is T)
                            return (T)component;
                        if (component is Entity)
                        {
                            return (component as Entity).GetFirstComponentOfType<T>(searchScope);                          
                        }
                    }
                    break;
            }       

            throw new MissingComponentException($"No component of required type {typeof(T).ToString()} exists inside component : {rootEntity.ToString()}");         
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
                    if (components == null)
                        components = new List<IComponent>();
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
                    throw new InvalidOperationException();
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
                            return component;
                        if (component is Entity)
                        {
                            foundComponent = (component as Entity).GetComponentById(id, searchScope);
                            if (foundComponent != null)
                                break;
                        }

                    }
                    break;
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
        public static IEnumerable<IComponent> GetComponentsByName(this Entity rootEntity, string name,SearchScope searchScope = SearchScope.Children)
        {

            switch (searchScope)
            {
                case SearchScope.Children:

                    var components = rootEntity.Components.Where(c => c.Name.Equals(name));
                    if (components == null)
                        components = new List<IComponent>();
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
                    throw new InvalidOperationException();
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
                }
                component.ResetComponent();
            }
            rootEntity.ResetComponent();
        }
       

        public static IEnumerable<ILoop> GetInnerLoops(this Entity rootEntity)
        {
            foreach (var component in rootEntity.Components)
            {
                if (!component.IsEnabled)
                    continue;

                if (component is ILoop)
                {
                    yield return (component as ILoop);
                    continue; //don't process inner childs of loop.it will take care of it's inner loops during execution itself
                }

                if (component is Entity)
                {
                    foreach (var item in GetInnerLoops(component as Entity))
                    {
                        yield return item;
                    }
                }
            }
        }

        //public static int GetMaxComponentId(this Entity rootEntity)
        //{          
        //    var allComponents = GetAllComponents(rootEntity);
        //    int maxComponentId = allComponents.Count() > 0 ? allComponents.Max(c => c.Id) : rootEntity.Id;
        //    return rootEntity.Id > maxComponentId ? rootEntity.Id : maxComponentId;
        //}


        public static T GetAnsecstorOfType<T>(this IComponent childComponent) where T : class,IComponent
        {
            var parent = childComponent.Parent;
            while(parent!=null)
            {               
                if (parent is T)
                    return (parent as T);
                parent = parent.Parent;
            }
            throw new MissingComponentException($"Ancestor of type :{typeof(T)} could not be located");
        }
    }
   
}
