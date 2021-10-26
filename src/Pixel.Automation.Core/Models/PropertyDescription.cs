using System;

namespace Pixel.Automation.Core.Models
{
    /// <summary>
    /// Captures the details of a script variable i.e. name and type
    /// </summary>
    public class PropertyDescription
    {
        /// <summary>
        /// Name of the property
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Type of the property
        /// </summary>
        public Type PropertyType { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyType"></param>
        public PropertyDescription(string propertyName, Type propertyType)
        {
            this.PropertyName = propertyName;
            this.PropertyType = propertyType;
        }
    }
}
