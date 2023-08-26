using Microsoft.AspNetCore.Components;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Pages.Projects;

public partial class StatisticsPage : ComponentBase
{
    [Inject]
    public IProjectService ProjectService { get; set; }

    [Parameter]
    public string ProjectId { get; set; }

    public IEnumerable<AutomationProject> AvailableProjects { get; set; }
    
    public AutomationProject SelectedProject { get; set; }

    protected override async Task OnInitializedAsync()
    {
        this.AvailableProjects = await this.ProjectService.GetProjectsAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrEmpty(this.ProjectId))
        {
            if(AvailableProjects.Any(a => a.ProjectId.Equals(this.ProjectId)))
            {
                this.SelectedProject = AvailableProjects.First(a => a.ProjectId.Equals(this.ProjectId));                
            }
        }
        else
        {
            this.SelectedProject = AvailableProjects.FirstOrDefault();
        }
        await Task.CompletedTask;
    }
}
