using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Charts
{
    public class XAxis
    {
        public string Type { get; set; }

        public List<string> Categories { get; set; } = new List<string>();
        
        public XAxis(string type, IEnumerable<string> categories)
        {
            this.Type = type;
            if(categories != null)
            {
                this.Categories.AddRange(categories);
            }
        }
    }
}
