using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Charts
{
    public class ChartDataModel
    {
        public string Width { get; set; } = "240px";

        public string Height { get; set; } = "240px";

        public List<string> Colors { get; set; } = new List<string>();

        public void AddColors(IEnumerable<string> colors)
        {
            this.Colors.AddRange(colors);
        }

        public PlotOptions PlotOptions { get; set; } = new PlotOptions();
    }
}
