using Pixel.Automation.Web.Portal.Charts;
using System.Collections.Generic;

namespace Pixel.Automation.Web.Portal.ViewModels
{
    public class SeriesChartDataViewModel<T> : ChartDataModel
    {
        public List<Series<T>> Series { get; } = new List<Series<T>>();

        public XAxis XAxis { get; set; }

        public SeriesChartDataViewModel(XAxis axisData)
        {
            this.XAxis = axisData;
        }

        public void AddSeries(string name, IEnumerable<T> data)
        {
            this.Series.Add(new Series<T>(name, data));
        }
    }
}
