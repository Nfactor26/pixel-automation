using Microsoft.AspNetCore.Components;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Automation.Web.Portal.ViewModels;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Core.Response;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Components.Tests;

public partial class TestStatistics : ComponentBase
{
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
            statisticsViewModel = await StatisticsService.GetTestStatisticsByTestId(TestId);
        }
    }

    async Task<PagedList<TestResult>> GetTestResultsAsync(TestResultRequest testResultRequest)
    {
        testResultRequest.TestId = TestId;
        var result = await TestResultService.GetTestResultsAsync(testResultRequest);
        return result;
    }
}
