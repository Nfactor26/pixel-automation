using Microsoft.AspNetCore.Components;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Automation.Web.Portal.ViewModels;
using Pixel.Persistence.Core.Enums;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Core.Response;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Components.Projects;

public partial class ProjectStatistics : ComponentBase
{
    private TestResultsTable resultsTable;

    [Inject]
    public IProjectStatisticsService Service { get; set; }

    [Inject]
    public ITestResultService TestResultService { get; set; }

    [Parameter]
    public AutomationProject Project { get; set; }

    ProjectStatisticsViewModel statisticsViewModel;

    protected override async Task OnParametersSetAsync()
    {
        if (Project != null)
        {
            statisticsViewModel = await Service.GetProjectStatisticsByProjectIdAsync(Project.ProjectId);
            resultsTable?.Reload();
        }
    }

    async Task<PagedList<TestResult>> GetTestResultsAsync(TestResultRequest testResultRequest)
    {
        testResultRequest.ProjectId = Project.ProjectId;
        testResultRequest.Result = TestStatus.Failed;
        var result = await TestResultService.GetTestResultsAsync(testResultRequest);
        return result;
    }
}
