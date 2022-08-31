using System;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class AllowedTypesAttribute : Attribute
    {
        public List<Type> Types { get; } = new  List<Type>();

        public AllowedTypesAttribute(params Type[] types)
        {
            this.Types.AddRange(types);
        }
    }
}
