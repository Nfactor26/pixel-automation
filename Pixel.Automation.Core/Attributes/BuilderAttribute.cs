using System;

namespace Pixel.Automation.Core.Attributes
{
    /// <summary>
    /// Builder Attribute captures type of Builder class that can be used to Create a properly configured instance
    /// of component that is decorated with this attribute.
    /// </summary>
    public class BuilderAttribute : Attribute
    {
        public Type Builder { get; }

        public BuilderAttribute(Type builderType)
        {
            this.Builder = builderType;
        }
    }
}
