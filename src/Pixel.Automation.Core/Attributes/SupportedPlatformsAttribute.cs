using System;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SupportedPlatformsAttribute : Attribute
    {
        public List<string> Platforms { get; } = new();

        public SupportedPlatformsAttribute(params string[] types)
        {
            this.Platforms.AddRange(types);
        }
    }
}
