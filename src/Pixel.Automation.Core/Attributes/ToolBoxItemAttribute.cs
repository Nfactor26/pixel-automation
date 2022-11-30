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
        /// <summary>
        /// Name of the ToolBox Item
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Category to which ToolBox Item belongs
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// SubCategory within Category
        /// </summary>
        public string SubCategory { get; }

        /// <summary>
        /// Custom Icon for the ToolBoxItem
        /// </summary>
        public string IconSource { get; }

        /// <summary>
        /// Description for the ToolBox Item
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Tags associated with ToolBox Item that can be used to filter items
        /// </summary>
        public List<string> Tags { get; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="category"></param>
        /// <param name="subCategory"></param>
        /// <param name="iconSource"></param>
        /// <param name="description"></param>
        /// <param name="tags"></param>
        public ToolBoxItemAttribute(string name, string category, string subCategory = "", string iconSource = null, string description = null, params string[] tags)
        {
            Name = name;
            Category = category;
            SubCategory = subCategory;
            IconSource = iconSource;
            Description = description;
            this.Tags = new List<string>();
            this.Tags.AddRange(tags);
        }
    }
}
