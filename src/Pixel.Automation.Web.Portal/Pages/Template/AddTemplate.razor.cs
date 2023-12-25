using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Pages.Template
{
    public partial class AddTemplate : ComponentBase
    {
        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public ISnackbar SnackBar { get; set; }

        [Inject]
        public IProjectService ProjectService { get; set; }

        [Inject]
        public ITemplateService TemplateService { get; set; }

        [Inject]
        public NavigationManager Navigator { get; set; }

        SessionTemplate sessionTemplate = new();
        List<AutomationProject> automationProjects = new();
        AutomationProject selectedProject;
        VersionInfo selectedVersion;

        protected override async Task OnInitializedAsync()
        {
            var projects = await ProjectService.GetProjectsAsync();
            automationProjects.AddRange(projects);
            await base.OnInitializedAsync();
        }

        private async  Task<IEnumerable<AutomationProject>> SearchProjects(string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                return await Task.FromResult(automationProjects);
            }
            return await  Task.FromResult(automationProjects.Where(s => s.Name.StartsWith(value) || s.Name.Contains(value)));
        }

        /// <summary>
        /// Add new application details
        /// </summary>
        /// <returns></returns>
        private async Task AddTemplateAsync()
        {
            sessionTemplate.ProjectId = selectedProject.ProjectId;
            sessionTemplate.ProjectName = selectedProject.Name;
            sessionTemplate.TargetVersion = selectedVersion.ToString();
            var result = await TemplateService.AddTemplateAsync(sessionTemplate);
            if (result.IsSuccess)
            {
                Navigator.NavigateTo($"templates/list");
                SnackBar.Add("Added successfully.", Severity.Success);
                return;
            }
            SnackBar.Add(result.ToString(), Severity.Error);
        }
    }
}
