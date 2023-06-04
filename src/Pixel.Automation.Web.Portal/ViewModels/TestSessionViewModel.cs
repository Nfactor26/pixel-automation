using Pixel.Automation.Web.Portal.Charts;
using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Automation.Web.Portal.ViewModels
{
    public class TestSessionViewModel
    {
        private TestSession testSession;

        public string SessionId => testSession.Id;

        public string ProjectId => testSession.ProjectId;

        public string ProjectName => testSession.ProjectName;

        public string ProjectVersion => testSession.ProjectVersion;

        public string TemplateName => testSession.TemplateName;

        public string MachineName => testSession.MachineName;

        public string OSDetails => testSession.OSDetails;


        public DateTime StartTime => testSession.SessionStartTime.ToLocalTime();

        public string TotalExecutionTime => String.Format("{0:0.000#}", testSession.SessionTime);

        public TestResultViewModel SlowestTest { get; private set; }

        public List<TestResultViewModel> TestsInSession { get; private set; } = new List<TestResultViewModel>();

        public TestResultViewModel FastestTest { get; private set; }

        public DonutChartDataViewModel<int> SummaryChart { get; set; }

        public SeriesChartDataViewModel<double> ExecutionTimeChart { get; set; }

        public TestSessionViewModel(TestSession testSession, IEnumerable<TestResult> testsInSession)
        {
            this.testSession = testSession;
            testsInSession = testsInSession.OrderBy(t => t.ExecutionOrder);
            foreach (var test in testsInSession)
            {
                this.TestsInSession.Add(new TestResultViewModel(test));
            }

            if(this.TestsInSession.Any())
            {
                SlowestTest = this.TestsInSession.Last();
                FastestTest = this.TestsInSession.First();
            }

            this.SummaryChart = new DonutChartDataViewModel<int>(new[] { "Passed", "Failed" }, new[] { testSession.NumberOfTestsPassed, testSession.NumberOfTestsFailed });
            this.SummaryChart.Height = "276px";
            this.SummaryChart.Width = "100%";

            var categories = new List<string>();
            var colors = new List<string>();
            var seriesData = new List<double>();
            foreach(var test in testsInSession)
            {
                categories.Add(test.TestName);
                seriesData.Add(test.ExecutionTime);
                if(test.Result == Persistence.Core.Enums.TestStatus.Success)
                {
                    colors.Add("#82EE5F");
                }
                else
                {
                    colors.Add("#E91E63");
                }
            }
            var xAxis = new XAxis("", categories);
            this.ExecutionTimeChart = new SeriesChartDataViewModel<double>(xAxis);
            this.ExecutionTimeChart.AddSeries("Time", seriesData);
            this.ExecutionTimeChart.AddColors(colors);
            this.ExecutionTimeChart.PlotOptions.Distributed = true;
            this.ExecutionTimeChart.Width = "100%";

        }
    }
}
