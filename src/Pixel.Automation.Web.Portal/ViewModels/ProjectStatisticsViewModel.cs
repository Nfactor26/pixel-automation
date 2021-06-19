using Pixel.Automation.Web.Portal.Charts;
using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Pixel.Automation.Web.Portal.ViewModels
{
    public class ProjectStatisticsViewModel
    {
        private ProjectStatistics projectStatistics;

        public string ProjectName => projectStatistics?.ProjectName;

        public int NumberOfTestsExeucted { get; set; }

        public int NumberOfTestsFailed { get; set; }

        public int NumberOfTestsPassed { get; set; }

        public double SuccessRate { get; set; }
    
        /// <summary>
        /// Average execution time for successful tests in seconds
        /// </summary>
        public double AvgExecutionTime { get; set; }

        public DonutChartDataViewModel<double> ExecutionSummaryChartData { get; set; }

        public SeriesChartDataViewModel<int> MonthlyExecutionHistoryData { get; set; }

        public SeriesChartDataViewModel<double> MonthlyAvgExecutionData { get; set; }

        public ProjectStatisticsViewModel()
        {

        }

        public ProjectStatisticsViewModel(ProjectStatistics projectStatistics)
        {
            this.projectStatistics = projectStatistics;
            if (projectStatistics != null && projectStatistics.MonthlyStatistics.Any())
            {
                NumberOfTestsExeucted = projectStatistics.MonthlyStatistics.Select(m => m.NumberOfTestsExecuted).Sum();
                NumberOfTestsPassed = projectStatistics.MonthlyStatistics.Select(m => m.NumberOfTestsPassed).Sum();
                NumberOfTestsFailed = projectStatistics.MonthlyStatistics.Select(m => m.NumberOfTestsFailed).Sum();
                if(NumberOfTestsPassed > 0)
                {
                    AvgExecutionTime = projectStatistics.MonthlyStatistics.Select(s => s.TotalExecutionTime).Sum() / NumberOfTestsPassed;
                }
                if (NumberOfTestsExeucted > 0)
                {
                    SuccessRate = ((double)NumberOfTestsPassed / NumberOfTestsExeucted) * 100.0;
                }

                this.ExecutionSummaryChartData = new DonutChartDataViewModel<double>(new string[] { "Passed", "Failed" }, new double[] { this.NumberOfTestsPassed, this.NumberOfTestsFailed });
                this.ExecutionSummaryChartData.Height = "284px";
                this.ExecutionSummaryChartData.Width = "100%";

                this.MonthlyExecutionHistoryData = GenerateMonthlyExecutionsHistoryData();
                this.MonthlyAvgExecutionData = GenerateMonthlyAvgExecutionData();
            }
        }

        public SeriesChartDataViewModel<int> GenerateMonthlyExecutionsHistoryData()
        {
            int currentYear = DateTime.Now.Year;
            int currentMonthOfYear = DateTime.Now.Month;
            List<string> monthsSoFar = new List<string>();
            for (int i = 1; i <= currentMonthOfYear; i++)
            {
                monthsSoFar.Add(DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(i));
            }
            XAxis xAxisData = new XAxis("category", monthsSoFar);

            SeriesChartDataViewModel<int> seriesData = new SeriesChartDataViewModel<int>(xAxisData);
            int[] passedSeries = new int[currentMonthOfYear];
            int[] failedSeries = new int[currentMonthOfYear];          
            for (int i = 1; i <= currentMonthOfYear; i++)
            {
                var executionStats = this.projectStatistics?.MonthlyStatistics?.FirstOrDefault(s => s.FromTime.ToLocalTime().Year.Equals(currentYear) && s.FromTime.ToLocalTime().Month.Equals(i));
                passedSeries[i - 1] = executionStats?.NumberOfTestsPassed ?? 0;
                failedSeries[i - 1] = executionStats?.NumberOfTestsFailed ?? 0;               
            }
            seriesData.AddSeries("Passed", "column", passedSeries);
            seriesData.AddSeries("Failed", "column", failedSeries);          
            seriesData.AddColors(new[] { "#82EE5F", "#E91E63"});
            seriesData.PlotOptions.Distributed = false;           
            seriesData.Width = "100%";
            return seriesData;
        }

        public SeriesChartDataViewModel<double> GenerateMonthlyAvgExecutionData()
        {
            int currentYear = DateTime.Now.Year;
            int currentMonthOfYear = DateTime.Now.Month;
            List<string> monthsSoFar = new List<string>();
            for (int i = 1; i <= currentMonthOfYear; i++)
            {
                monthsSoFar.Add(DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(i));
            }
            XAxis xAxisData = new XAxis("category", monthsSoFar);

            SeriesChartDataViewModel<double> seriesData = new SeriesChartDataViewModel<double>(xAxisData);
            double[] avgExecutionTimeSeries = new double[currentMonthOfYear];         
            for (int i = 1; i <= currentMonthOfYear; i++)
            {
                var executionStats = this.projectStatistics?.MonthlyStatistics?.FirstOrDefault(s => s.FromTime.ToLocalTime().Year.Equals(currentYear) && s.FromTime.ToLocalTime().Month.Equals(i));
                avgExecutionTimeSeries[i - 1] = executionStats?.AvgExecutionTime ?? 0;               
            }
            seriesData.AddSeries("Avg Execution Time", avgExecutionTimeSeries);           
            seriesData.PlotOptions.Distributed = false;
            seriesData.Height = "248px";
            seriesData.Width = "100%";

            return seriesData;
        }
    }

}
