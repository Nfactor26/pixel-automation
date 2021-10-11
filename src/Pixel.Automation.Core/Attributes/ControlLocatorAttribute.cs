using System;

namespace Pixel.Automation.Core.Attributes
{
    /// <summary>
    /// When a control identity is dropped on process designer, we check if the LocatorType is already added to process designer.
    /// If not create an instance of LocatorType and add it to the process designer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ControlLocatorAttribute : Attribute
    {
        public Type LocatorType { get; }

        public ControlLocatorAttribute(Type locatorType)
        {
            this.LocatorType = locatorType;
        }
    }
}
