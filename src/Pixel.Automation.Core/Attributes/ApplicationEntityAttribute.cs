using System;

namespace Pixel.Automation.Core.Attributes
{
    /// <summary>
    /// Different <see cref="Application"/> implementations are decorated with ApplicationEntityAttribute.
    /// When an Application is dropped on process designer , an instance of ApplicationEntity will be created instead and added to the process.
    /// </summary>
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
