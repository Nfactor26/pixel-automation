using System;

namespace Pixel.Automation.Core.Attributes
{
    /// <summary>
    /// Ninject is configured to use this Attribute for dependency injection
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]   
    public class InjectedAttribute : Attribute
    {
        public InjectedAttribute()
        {
        }
    }
}
