using Pixel.Automation.Core.Enums;
using System;

namespace Pixel.Automation.Core.Attributes
{
    /// <summary>
    /// While generating a data model code for Prefabs, properties are marked with ParameterUsageAttribute.
    /// This is used while mapping input and ouput scripts for Prefab to make better decisions to generated the mapping code.
    /// </summary>
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
