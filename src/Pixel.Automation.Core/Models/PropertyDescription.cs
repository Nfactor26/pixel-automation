using System;

namespace Pixel.Automation.Core.Models
{
    public class PropertyDescription
    {
        public string PropertyName { get; private set; }

        public Type PropertyType { get; private set; }

        public PropertyDescription(string propertyName, Type propertyType)
        {
            this.PropertyName = propertyName;
            this.PropertyType = propertyType;
        }
    }
}
