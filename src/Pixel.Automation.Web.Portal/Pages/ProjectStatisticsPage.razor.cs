using Microsoft.AspNetCore.Components;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Automation.Web.Portal.ViewModels;
using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Pages
{
    public partial class ProjectStatisticsPage : ComponentBase
    {
        [Inject]
        public IProjectStatisticsService Service { get; set; }

        [Parameter]
        public string ProjectId { get; set; }

        ProjectStatisticsViewModel statisticsViewModel;

        IEnumerable<TestStatistics> recentFailures;     

        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrEmpty(ProjectId))
            {
                statisticsViewModel = await Service.GetProjectStatisticsByProjectIdAsync(ProjectId);
                recentFailures = await Service.GetRecentFailuresAsync(ProjectId);
            }
        }
    }
}
