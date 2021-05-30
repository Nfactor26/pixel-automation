using Pixel.Automation.Web.Portal.Charts;
using System.Collections.Generic;

namespace Pixel.Automation.Web.Portal.ViewModels
{
    public class DonutChartDataViewModel<T> : ChartDataModel
    {
        public List<string> Labels { get; set; } = new List<string>();

        public List<T> Series { get; set; } = new List<T>(); 

        public DonutChartDataViewModel(IEnumerable<string> labels, IEnumerable<T> values)
        {
            this.Labels.AddRange(labels);
            this.Series.AddRange(values);            
        }
    }
}
