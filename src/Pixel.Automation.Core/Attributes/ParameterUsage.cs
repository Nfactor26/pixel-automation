using Pixel.Automation.Core.Enums;
using System;

namespace Pixel.Automation.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ParameterUsageAttribute : Attribute
    {
        public  ParameterUsage ParameterUsage { get; }

        public ParameterUsageAttribute(ParameterUsage parameterUsage)
        {
            this.ParameterUsage = parameterUsage;
        }
    }
}
