using System;

namespace Pixel.Automation.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredComponentAttribute : Attribute
    {
        public RequiredComponentAttribute()
        {
            
        }
    }    
}
