using Pixel.Automation.Web.Portal.Charts;
using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.ViewModels
{
    public class TestStatisticsViewModel
    {
        private TestStatistics testStatistics;
              
        public double MinExecutionTime { get; set; }

        public double MaxExecutionTime { get; set; }

        public double AvgExecutionTime { get; set; }

        public double MinExecutionTimeForCurrentMonth { get; set; }

        public double MaxExecutionTimeForCurrentMonth { get; set; }

        public double AvgExecutionTimeForCurrentMonth { get; set; }
             
        public int NumberOfTimesExeucted { get; set; }     
    
        public int NumberOfTimesFailed { get; set; }
          
        public int NumberOfTimesPassed { get; set; }
        
        public double SuccessRate { get; set; }

        public IEnumerable<FailureDetails> UniqueFailures => testStatistics?.UniqueFailures ?? Enumerable.Empty<FailureDetails>();

        public DonutChartDataViewModel<double> ExecutionSummaryChartData { get; set; }

        public SeriesChartDataViewModel<int> MonthlyExecutionHistoryData { get; set; }
       
        public TestStatisticsViewModel()
        {

        }

        public TestStatisticsViewModel(TestStatistics testStatistics)
        {
            this.testStatistics = testStatistics;
          
            if(testStatistics != null && testStatistics.MonthlyStatistics.Any())
            {
                this.MinExecutionTime = testStatistics.MonthlyStatistics.Select(s => s.MinExecutionTime).Min();
                this.MaxExecutionTime = testStatistics.MonthlyStatistics.Select(s => s.MaxExecutionTime).Min();
                this.AvgExecutionTime = testStatistics.MonthlyStatistics.Select(s => s.AvgExecutionTime).Min();
                this.MinExecutionTimeForCurrentMonth = testStatistics.MonthlyStatistics.Last().MinExecutionTime;
                this.MaxExecutionTimeForCurrentMonth = testStatistics.MonthlyStatistics.Last().MaxExecutionTime;
                this.AvgExecutionTimeForCurrentMonth = testStatistics.MonthlyStatistics.Last().AvgExecutionTime;
                this.NumberOfTimesExeucted = testStatistics.MonthlyStatistics.Select(m => m.NumberOfTimesExecuted).Sum();
                this.NumberOfTimesFailed = testStatistics.MonthlyStatistics.Select(m => m.NumberOfTimesFailed).Sum();
                this.NumberOfTimesPassed =  testStatistics.MonthlyStatistics.Select(m => m.NumberOfTimesPassed).Sum();
                this.SuccessRate = (NumberOfTimesPassed / NumberOfTimesExeucted) * 100;

                this.ExecutionSummaryChartData = new DonutChartDataViewModel<double>(new string[] { "Passed", "Failed" }, new double[] { this.NumberOfTimesPassed, this.NumberOfTimesFailed });
                this.ExecutionSummaryChartData.Height = "276px";
                this.ExecutionSummaryChartData.Width = "100%";

                this.MonthlyExecutionHistoryData = GenerateMonthlyExecutionsHistoryData();
            }
        }


        public SeriesChartDataViewModel<int> GenerateMonthlyExecutionsHistoryData()
        {
            int currentYear = DateTime.Now.Year;
            int currentMonthOfYear = DateTime.Now.Month;
            List<string> monthsSoFar = new List<string>();
            for(int i = 1; i <= currentMonthOfYear; i++)
            {
                monthsSoFar.Add(DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(i));
            }
            XAxis xAxisData = new XAxis("category", monthsSoFar);

            SeriesChartDataViewModel<int> seriesData = new SeriesChartDataViewModel<int>(xAxisData);
            int[] passedSeries = new int[currentMonthOfYear];
            int[] failedSeries = new int[currentMonthOfYear];
            for (int i = 1; i <= currentMonthOfYear; i++)
            {
                var executionStats = this.testStatistics?.MonthlyStatistics?.FirstOrDefault(s => s.FromTime.ToLocalTime().Year.Equals(currentYear) && s.FromTime.ToLocalTime().Month.Equals(i));
                passedSeries[i - 1] = executionStats?.NumberOfTimesPassed ?? 0;
                failedSeries[i - 1] = executionStats?.NumberOfTimesFailed ?? 0;
            }
            seriesData.AddSeries("Passed", passedSeries);
            seriesData.AddSeries("Failed", failedSeries);
            seriesData.AddColors(new[] { "#82EE5F", "#E91E63" });
            seriesData.PlotOptions.Distributed = false;

            return seriesData;
        }

      
    }
}
