using Pixel.Automation.Web.Portal.Charts;
using Pixel.Automation.Web.Portal.Helpers;
using Pixel.Persistence.Core.Models;
using System;
using System.Linq;

namespace Pixel.Automation.Web.Portal.ViewModels
{
    public class TestStatisticsViewModel
    {
        private TestStatistics testStatistics;

        public string TestName { get; private set; }
        
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

        public DonutChartDataViewModel<double> ExecutionSummaryChartData { get; set; }

        public SeriesChartDataViewModel<int> MonthlyExecutionHistoryData { get; set; }

        public SeriesChartDataViewModel<double> MonthlyAvgExecutionData { get; set; }

        public TestStatisticsViewModel()
        {

        }

        public TestStatisticsViewModel(TestStatistics testStatistics)
        {
            this.testStatistics = testStatistics;
            this.TestName = testStatistics.TestName;
            if(testStatistics != null && testStatistics.MonthlyStatistics.Any())
            {
                this.MinExecutionTime = testStatistics.MonthlyStatistics.Select(s => s.MinExecutionTime).Min();
                this.MaxExecutionTime = testStatistics.MonthlyStatistics.Select(s => s.MaxExecutionTime).Min();              
                this.MinExecutionTimeForCurrentMonth = testStatistics.MonthlyStatistics.Last().MinExecutionTime;
                this.MaxExecutionTimeForCurrentMonth = testStatistics.MonthlyStatistics.Last().MaxExecutionTime;
                this.AvgExecutionTimeForCurrentMonth = testStatistics.MonthlyStatistics.Last().AvgExecutionTime;
                this.NumberOfTimesExeucted = testStatistics.MonthlyStatistics.Select(m => m.NumberOfTimesExecuted).Sum();
                this.NumberOfTimesFailed = testStatistics.MonthlyStatistics.Select(m => m.NumberOfTimesFailed).Sum();
                this.NumberOfTimesPassed =  testStatistics.MonthlyStatistics.Select(m => m.NumberOfTimesPassed).Sum();

                if (this.NumberOfTimesPassed > 0)
                {                   
                    this.AvgExecutionTime = testStatistics.MonthlyStatistics.Select(s => s.TotalExecutionTime).Sum() / NumberOfTimesPassed;

                }

                if (this.NumberOfTimesExeucted > 0)
                {
                    this.SuccessRate = ((double)NumberOfTimesPassed / NumberOfTimesExeucted) * 100.0;
                }               

                this.ExecutionSummaryChartData = new DonutChartDataViewModel<double>(new string[] { "Passed", "Failed" }, new double[] { this.NumberOfTimesPassed, this.NumberOfTimesFailed });
                this.ExecutionSummaryChartData.Height = "276px";
                this.ExecutionSummaryChartData.Width = "100%";

                this.MonthlyExecutionHistoryData = GenerateMonthlyExecutionsHistoryData();
                this.MonthlyAvgExecutionData = GenerateMonthlyAvgExecutionData();
            }
        }

        /// <summary>
        /// Generate monthly execution history data for last 6 months
        /// </summary>
        /// <returns></returns>
        public SeriesChartDataViewModel<int> GenerateMonthlyExecutionsHistoryData()
        {
            int currentYear = DateTime.Now.Year;
            int currentMonthOfYear = DateTime.Now.Month;
            var monthsSoFar = DateTimeHelper.GetLastNMonths(6);
            XAxis xAxisData = new XAxis("category", monthsSoFar);

            SeriesChartDataViewModel<int> seriesData = new SeriesChartDataViewModel<int>(xAxisData);
            int[] passedSeries = new int[6];
            int[] failedSeries = new int[6];
            currentMonthOfYear = DateTime.Now.Month;
            for (int i = 6; i >=1; i--)
            {              
                var executionStats = this.testStatistics?.MonthlyStatistics?.Where(s => s.FromTime.ToLocalTime().Year.Equals(currentYear) && s.FromTime.ToLocalTime().Month.Equals(currentMonthOfYear));
                foreach (var executionStat in executionStats ?? Enumerable.Empty<TestExecutionStatistics>())
                {
                    passedSeries[i - 1] += executionStat.NumberOfTimesPassed;
                    failedSeries[i - 1] += executionStat.NumberOfTimesFailed;
                }
                currentMonthOfYear--;
                if (currentMonthOfYear == 0)
                {
                    currentMonthOfYear = 12;
                    currentYear--;
                }
            }
            seriesData.AddSeries("Passed", passedSeries);
            seriesData.AddSeries("Failed", failedSeries);
            seriesData.AddColors(new[] { "#82EE5F", "#E91E63" });
            seriesData.PlotOptions.Distributed = false;
            seriesData.Width = "100%";

            return seriesData;
        }

        public SeriesChartDataViewModel<double> GenerateMonthlyAvgExecutionData()
        {
            int currentYear = DateTime.Now.Year;
            int currentMonthOfYear = DateTime.Now.Month;
            var monthsSoFar = DateTimeHelper.GetLastNMonths(6);
            XAxis xAxisData = new XAxis("category", monthsSoFar);

            SeriesChartDataViewModel<double> seriesData = new SeriesChartDataViewModel<double>(xAxisData);
            currentMonthOfYear = DateTime.Now.Month;
            double[] avgExecutionTimeSeries = new double[6];
            for (int i = 6; i >= 1; i--)
            {
                var executionStats = this.testStatistics?.MonthlyStatistics?.Where(s => s.FromTime.ToLocalTime().Year.Equals(currentYear) && s.FromTime.ToLocalTime().Month.Equals(currentMonthOfYear))
                    ?? Enumerable.Empty<TestExecutionStatistics>();
                if (executionStats.Sum(e => e.NumberOfTimesPassed) > 0)
                {
                    avgExecutionTimeSeries[i - 1] = executionStats.Sum(e => e.TotalExecutionTime) / executionStats.Sum(e => e.NumberOfTimesPassed);
                }               
                currentMonthOfYear--;
                if (currentMonthOfYear == 0)
                {
                    currentMonthOfYear = 12;
                    currentYear--;
                }
            }
            seriesData.AddSeries("Avg Execution Time", avgExecutionTimeSeries);
            seriesData.PlotOptions.Distributed = false;
            seriesData.Height = "248px";
            seriesData.Width = "100%";

            return seriesData;
        }

    }
}
