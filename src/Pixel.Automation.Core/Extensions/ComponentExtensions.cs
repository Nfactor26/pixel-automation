using Pixel.Automation.Core.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core
{
    public static class ComponentExtensions
    {
        /// <summary>
        /// Always update DisplayAttribute in property getter. Otherwise, if there are two components of same type, changing property on one impacts other as well.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetDispalyAttribute(this object component, string propertyName, bool value)
        {
            var displayAttr = TypeDescriptor.GetProperties(component.GetType())[propertyName]?.Attributes[typeof(DisplayAttribute)] as DisplayAttribute;
            if (displayAttr != null)
            {
                displayAttr.AutoGenerateField = value;
                return;
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
    }
}
