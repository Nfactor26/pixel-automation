using MudBlazor;
using Pixel.Automation.Web.Portal.ViewModels;
using Pixel.Persistence.Core.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Services
{
    public interface ITestStatisticsService
    {
        Task<TestStatisticsViewModel> GetTestStatisticsByTestId(string testId);
    }

    public class TestStatisticsService : ITestStatisticsService
    {
        private readonly HttpClient http;
        private readonly ISnackbar snackBar;

        public TestStatisticsService(HttpClient http, ISnackbar snackBar)
        {
            this.http = http;
            this.snackBar = snackBar;
        }

        public async Task<TestStatisticsViewModel> GetTestStatisticsByTestId(string testId)
        {
            try
            {
                var statistics = await this.http.GetFromJsonAsync<TestStatistics>($"/api/TestStatistics/{testId}");
                var statisticsViewModel = new TestStatisticsViewModel(statistics);
                return statisticsViewModel;
            }
            catch (Exception ex)
            {
                snackBar.Add($"Error while retrieving test statistic details for test id : {testId}", Severity.Error);
                Console.WriteLine("Error: " + ex.ToString());
            }
            return null;
        }
    }
}
