using System;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces
{
    /// <summary>
    /// Scoped entities can specify a complex property from DataModel to act as a local variable scope.
    /// When argument uses data bound mode, only properties from this complex object will be visible.  
    /// </summary>
    public interface IScopedEntity
    {
        /// <summary>
        /// Get the object instance which should act as a local scope for child component of this scoped entity
        /// For ex : For Loop entity will have current object iterated as scoped type instance
        /// </summary>
        /// <returns></returns>
        object GetScopedTypeInstance();


        /// <summary>
        /// Gets the name of the property in DataModel to which local scope is bound.
        /// </summary>
        /// <returns></returns>
        string GetScopedArgumentName();

        /// <summary>
        /// Get all the properties of a given type from scoped type
        /// </summary>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        IEnumerable<string> GetPropertiesOfType(Type propertyType);        
    }
}
