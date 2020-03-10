using System;

namespace Pixel.Automation.Core.Attributes
{
    public class ControlLocatorAttribute : Attribute
    {
        public Type RequiredControlLocatorType { get; }

        public ControlLocatorAttribute(Type requiredControlLocatorType)
        {
            this.RequiredControlLocatorType = requiredControlLocatorType;
        }
    }
}
