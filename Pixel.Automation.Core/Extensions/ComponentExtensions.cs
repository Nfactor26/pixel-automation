using Pixel.Automation.Core.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core
{
    public static class ComponentExtensions
    {
        public static void SetBrowsableAttribute(this object component,string propertyName,bool value)
        {
            var attr = TypeDescriptor.GetProperties(component.GetType())[propertyName]?.Attributes[typeof(BrowsableAttribute)] as BrowsableAttribute;
            attr?.GetType().GetField("Browsable", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(attr, value);
        }

        public static void SetDispalyAttribute(this object component, string propertyName, bool value)
        {
            var displayAttr = TypeDescriptor.GetProperties(component.GetType())[propertyName]?.Attributes[typeof(DisplayAttribute)] as DisplayAttribute;
            if (displayAttr != null)
            {
                displayAttr.AutoGenerateField = value;
                return;
            }
        }

        public static void SetReadOnlyAttribute(this object component, string propertyName, bool value)
        {
            var attr = System.ComponentModel.TypeDescriptor.GetProperties(component.GetType())[propertyName]?.Attributes[typeof(System.ComponentModel.ReadOnlyAttribute)] as System.ComponentModel.ReadOnlyAttribute;
            attr?.GetType().GetField("IsReadOnly", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(attr, value);
        }

        public static T CreateCopy<T>(this IComponent component) where T : IComponent
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, component);
                stream.Seek(0, SeekOrigin.Begin);
                T copy = (T)formatter.Deserialize(stream);              
                return copy;
            }
        }

        /// <summary>
        /// Get scoped entity for a component. A component which is child of a scoped entity e.g. foreach loop 
        /// will have additional local variables visible in bound mode (from current iteration item)
        /// </summary>
        /// <param name="component"></param>
        /// <param name="scopedEntity"></param>
        /// <returns></returns>
        public static bool TryGetScopedParent(this IComponent component, out IScopedEntity scopedEntity)
        {
            IComponent parent = component.Parent;
            while (parent != null)
            {
                if (parent is IScopedEntity)
                {
                    scopedEntity = parent as IScopedEntity;
                    return true;
                }
                parent = parent.Parent;
            }
            scopedEntity = null;
            return false;
        }

        public static IArgumentProcessor GetArgumentProcessor(this IComponent component)
        {
            //component.TryGetScopedParent(out IScopedEntity scopedEntity);
            return (component as Component).EntityManager.GetArgumentProcessor(null);
        }
    }
}
