using Microsoft.AspNetCore.Components;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Automation.Web.Portal.ViewModels;
using Pixel.Persistence.Core.Enums;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Core.Response;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Pages
{
    public partial class ProjectStatisticsPage : ComponentBase
    {
        [Inject]
        public IProjectStatisticsService Service { get; set; }

        [Inject]
        public ITestResultService TestResultService { get; set; }

        [Parameter]
        public string ProjectId { get; set; }

        ProjectStatisticsViewModel statisticsViewModel;
        
        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrEmpty(ProjectId))
            {
                statisticsViewModel = await Service.GetProjectStatisticsByProjectIdAsync(ProjectId);               
            }
        }

        async Task<PagedList<TestResult>> GetTestResultsAsync(TestResultRequest testResultRequest)
        {
            testResultRequest.ProjectId = ProjectId;
            testResultRequest.Result = TestStatus.Failed;
            var result = await TestResultService.GetTestResultsAsync(testResultRequest);
            return result;
        }
    }
}
