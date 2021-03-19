using System;
using System.Collections.Generic;

namespace Pixel.Automation.Editor.Core
{
    public class ComponentToolBoxItem
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Description { get; set; }
        public string IconSource { get; set; }
        public List<string> Tags { get; set; }
        public Type TypeOfComponent { get; set; }

    }
}
