using System;

namespace Pixel.Automation.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ContainerEntityAttribute : Attribute
    {
        public Type ContainerEntityType { get; }

        public ContainerEntityAttribute(Type containerEntityType)
        {
            this.ContainerEntityType = containerEntityType;
        }
    }
}
