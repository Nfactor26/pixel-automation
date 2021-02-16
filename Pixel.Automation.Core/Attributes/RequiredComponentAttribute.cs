using System;

namespace Pixel.Automation.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RequiredComponentAttribute : Attribute
    {
        public RequiredComponentAttribute()
        {
            
        }
    }    
}
