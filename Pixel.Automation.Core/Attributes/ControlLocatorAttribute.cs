using System;

namespace Pixel.Automation.Core.Attributes
{
    public class ControlLocatorAttribute : Attribute
    {
        public Type LocatorType { get; }

        public ControlLocatorAttribute(Type locatorType)
        {
            this.LocatorType = locatorType;
        }
    }
}
