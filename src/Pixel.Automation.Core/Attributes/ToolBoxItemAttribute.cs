using System;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Attributes
{
    /// <summary>
    /// Any component decorated with this attribute will appear in the components tool box provide by the designer application.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ToolBoxItemAttribute : Attribute
    {
        public string Name { get; }
        public string Category { get; }
        public string SubCategory { get; }
        public string IconSource { get; }
        public string Description { get; }
        public List<string> Tags { get; }

        public ToolBoxItemAttribute(string name, string category, string subCategory = "", string iconSource = null, string description = null, params string[] tags)
        {
            Name = name;
            Category = category;
            SubCategory = subCategory;
            IconSource = iconSource;
            Description = null;
            this.Tags = new List<string>();
            this.Tags.AddRange(tags);
        }
    }
}
