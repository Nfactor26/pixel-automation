using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Automation.Web.Portal.ViewModels;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Core.Response;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Pages
{
    public partial class TestStatisticsPage : ComponentBase
    {
        public string Search { get; set; }

        [Inject]
        public ITestResultService TestResultService { get; set; }

        [Inject]
        public ITestStatisticsService StatisticsService { get; set; }

        [Parameter]
        public string TestId { get; set; }

        private TestStatisticsViewModel statisticsViewModel = new TestStatisticsViewModel();

        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrEmpty(TestId))
            {
                this.statisticsViewModel = await StatisticsService.GetTestStatisticsByTestId(TestId);
            }
        }
        async Task<PagedList<TestResult>> GetTestResultsAsync(TestResultRequest testResultRequest)
        {
            testResultRequest.TestId = TestId;
            var result = await TestResultService.GetTestResultsAsync(testResultRequest);
            return result;
        }
    }
}
