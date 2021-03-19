using System;

namespace Pixel.Automation.Core.Attributes
{
    /// <summary>
    /// Intializer Attribute captures type of Intializer class that can be used to initialize a component once it is created  
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class InitializerAttribute : Attribute
    {
        public Type Initializer { get; }

        public InitializerAttribute(Type initializerType)
        {
            this.Initializer = initializerType;
        }
    }
}
