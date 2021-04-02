using System;

namespace Pixel.Automation.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ApplicationEntityAttribute : Attribute
    {
        public Type ApplicationEntity { get; }

        public ApplicationEntityAttribute(Type applicationEntity)
        {
            this.ApplicationEntity = applicationEntity;
        }
    }
}
