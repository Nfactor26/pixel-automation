using System;

namespace Pixel.Automation.Core.Attributes
{
    /// <summary>
    /// When a class decorated with ContainerEntityAttribute is dropped to process designer, the ContainerEntityType isntance is created 
    /// and added instead of the actual target being dropped e.g. we will add a ControlEntity when a ControlIdentity is being dropped    
    /// </summary>
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
