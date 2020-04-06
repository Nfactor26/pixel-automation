using System;
using System.Linq;
using System.Reflection;

namespace Pixel.Automation.Core.Extensions
{
    public static class TypeExtensions
    {
        public static string GetDisplayName(this Type type)
        {
            switch (type.IsGenericType)
            {
                case true:
                    if (type.ContainsGenericParameters)
                        return type.Name.Split('`')[0];
                    else
                        return type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(x => GetDisplayName(x)).ToArray()) + ">";
                case false:
                    return type.Name;
            }          
        }

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
    }
}
