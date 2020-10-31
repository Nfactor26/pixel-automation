using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pixel.Automation.Core.Extensions
{
    public static class TypeExtensions
    {
        private static readonly string referenceTemplate = "#r \"{0}\"";
        private static readonly string usingTemplate = "using {0};";

        /// <summary>
        /// Get friendly display name for a given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetDisplayName(this Type type)
        {
            switch (type.IsGenericType)
            {
                case true:
                    if (type.ContainsGenericParameters)
                    {
                        return type.Name.Split('`')[0];
                    }                    
                    else
                    {
                        return type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(x => GetDisplayName(x)).ToArray()) + ">";
                    }
                case false:
                    return type.Name;
            }          
        }

        /// <summary>
        /// Get value of a specified property using reflection
        /// </summary>
        /// <typeparam name="T">Type of property</typeparam>
        /// <param name="instance">Object which contains the property</param>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Value of specified property of type T</returns>
        public static T GetPropertyValue<T>(this object instance, string propertyName)
        {
            PropertyInfo sourceProperty = instance.GetType().GetProperty(propertyName);
            if (sourceProperty == null)
            {
                throw new ArgumentException($"Property : {propertyName} doesn't exist on object of type : {instance.GetType()}");
            }

            //when type can be directly assigned
            if (typeof(T).IsAssignableFrom(sourceProperty.PropertyType))
            {
                T targetPropertyValue = (T)sourceProperty.GetValue(instance);
                return targetPropertyValue;
            }
            throw new Exception($"Data type : {typeof(T)} is not assignable from property {propertyName} on object");
        }

        /// <summary>
        /// Set value of a specified property using reflection
        /// </summary>
        /// <typeparam name="T">Type of property</typeparam>
        /// <param name="instance">Object which contains the property</param>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="valueToSet">Value to set</param>
        public static  void SetPropertyValue<T>(this object instance, string propertyName, T valueToSet)
        {
            PropertyInfo targetProperty = instance.GetType().GetProperty(propertyName);
            if (targetProperty == null)
            {
                throw new ArgumentException($"Property : {propertyName} doesn't exist on object of type : {instance.GetType()}");
            }
            if (targetProperty.PropertyType.IsAssignableFrom(typeof(T)))    //when type can be directly assigned
            {
                targetProperty.SetValue(instance, valueToSet);
                return;
            }

            throw new Exception($"Data type : {typeof(T)} is not assignable to property {propertyName} on object");
        }


        public static string GetRequiredImportsForType(this Type type, IEnumerable<Assembly> withRequiredRefrences, IEnumerable<string> withRequiredDirectives)
        {
            StringBuilder result = new StringBuilder();
            var dllReferences = type.GetDllReferences(withRequiredRefrences);
            foreach (var reference in dllReferences)
            {
                result.Append(string.Format(referenceTemplate, reference));
                result.Append(Environment.NewLine);
            }

            var usingDirectives = type.GetUsingDirectives(withRequiredDirectives);
            foreach (var usingDirective in usingDirectives)
            {
                result.Append(string.Format(usingTemplate, usingDirective));
                result.Append(Environment.NewLine);
            }          
            return result.ToString();
        }



        public static IEnumerable<string> GetDllReferences(this Type type, IEnumerable<Assembly> withRequiredRefrences)
        {
            List<string> distinctReferences = new List<string>();

            foreach(var assembly in withRequiredRefrences.Distinct())
            {
                if (assembly.Location.StartsWith(Environment.CurrentDirectory) && !assembly.Location.Contains("Temp"))
                {
                    distinctReferences.Add(Path.GetFileName(assembly.Location));
                }
            }

            if(!withRequiredRefrences.Contains(type.Assembly))
            {
                string containedInDll = type.Assembly.Location;
                //include those in application directory but not in Temp folder i.e. dynamically compiled dll's by project.
                if (containedInDll.StartsWith(Environment.CurrentDirectory) && !containedInDll.Contains("Temp"))
                {
                    distinctReferences.Add(Path.GetFileName(containedInDll));
                }
            }
           

            if (type.IsGenericType)
            {
                foreach (var typeArgument in type.GenericTypeArguments)
                {
                    if (!withRequiredRefrences.Contains(typeArgument.Assembly))
                    {
                        string containedInDll = typeArgument.Assembly.Location;
                        if (containedInDll.StartsWith(Environment.CurrentDirectory) && !containedInDll.Contains("Temp") && !distinctReferences.Contains(Path.GetFileName(containedInDll)))
                        {
                            distinctReferences.Add(Path.GetFileName(containedInDll));
                        }
                    }                   
                }
            }

            return distinctReferences;

        }

        public static IEnumerable<string> GetUsingDirectives(this Type type, IEnumerable<string> withRequiredDirectives)
        {
            List<string> distinceDirectives = new List<string>();    
            
            foreach(var directive in withRequiredDirectives.Distinct())
            {
                distinceDirectives.Add(directive);
            }

            if(!distinceDirectives.Contains(type.Namespace))
            {
                distinceDirectives.Add(type.Namespace);
            }

            if (type.IsGenericType)
            {
                foreach (var typeArgument in type.GenericTypeArguments)
                {
                    if (!distinceDirectives.Contains(typeArgument.Namespace))
                    {
                        distinceDirectives.Add(typeArgument.Namespace);
                    }
                }
            }
            return distinceDirectives;
        }
    }
}
