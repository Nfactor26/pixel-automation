using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pixel.Automation.Web.Portal.Components.Triggers;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Persistence.Core.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Pages.Template
{
    public partial class EditTemplate : ComponentBase
    {       
        [Parameter]
        public string TemplateId { get; set; }

        [Inject]
        public ITemplateService TemplateService { get; set; }

        [Inject]
        public ITriggerService TriggerService { get; set; }
     
        [Inject]
        public IProjectService ProjectService { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public ISnackbar SnackBar { get; set; }

        [Inject]
        public NavigationManager Navigator { get; set; }

        SessionTemplate sessionTemplate;
        AutomationProject selectedProject;   
        bool hasErrors = false;

        /// <summary>
        /// Fetch template details when id is set
        /// </summary>
        /// <returns></returns>
        protected override async Task OnParametersSetAsync()
        {
            this.sessionTemplate = await GetTemplateAsync(this.TemplateId);
            this.selectedProject = await GetProjectAsync(this.sessionTemplate.ProjectId);       
        }

        /// <summary>
        /// Update template
        /// </summary>
        /// <returns></returns>
        async Task UpdateTemplatesAsync()
        {           
            var result = await TemplateService.UpdateTemplateAsync(sessionTemplate);
            if (result.IsSuccess)
            {
                SnackBar.Add("Updated successfully.", Severity.Success);
                this.sessionTemplate = await GetTemplateAsync(this.TemplateId);
                return;
            }
            SnackBar.Add(result.ToString(), Severity.Error);
        }

        /// <summary>
        /// Retrieve template details given it's id
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        private async Task<SessionTemplate> GetTemplateAsync(string templateId)
        {
            if (!string.IsNullOrEmpty(templateId))
            {
                try
                {
                    return await TemplateService.GetTemplateByIdAsync(templateId);
                }
                catch (Exception ex)
                {
                    hasErrors = true;
                    SnackBar.Add($"Failed to retrieve template data. {ex.Message}", Severity.Error);
                }
            }
            else
            {
                hasErrors = true;
                SnackBar.Add("No templateId specified.", Severity.Error);
            }
            return null;
        }

        /// <summary>
        /// Retrieve project details given it's id
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        private async Task<AutomationProject> GetProjectAsync(string projectId)
        {
            if (!string.IsNullOrEmpty(projectId))
            {
                try
                {
                    return await ProjectService.GetProjectByIdAsync(projectId);
                }
                catch (Exception ex)
                {
                    hasErrors = true;
                    SnackBar.Add($"Failed to retrieve automation project details. {ex.Message}", Severity.Error);
                }
            }
            else
            {
                hasErrors = true;
                SnackBar.Add("No projectId specified.", Severity.Error);
            }
            return null;
        }

        /// <summary>
        /// Show a dialog to create and add a new trigger to the template
        /// </summary>
        /// <returns></returns>
        async  Task AddTriggerAsync()
        {
            var parameters = new DialogParameters
            {
                { "TemplateId", sessionTemplate.Id },
                { "ExistingTriggers", sessionTemplate.Triggers },
                { "Service", TriggerService }
            };
            var dialog = DialogService.Show<AddCronTrigger>("Add Trigger", parameters, new DialogOptions() { MaxWidth = MaxWidth.ExtraLarge, CloseButton = true });
            var result = await dialog.Result;
            if (!result.Canceled && result.Data is SessionTrigger trigger)
            {
                sessionTemplate.Triggers.Add(trigger);
                SnackBar.Add($"Trigger was added.", Severity.Success);
            }
        }

        /// <summary>
        /// Update details of existing trigger
        /// </summary>
        /// <param name="original"></param>
        /// <param name="modified"></param>
        /// <returns></returns>
        async Task EditTriggerAsync(SessionTrigger trigger)
        {
            var parameters = new DialogParameters
            {
                { "TemplateId", sessionTemplate.Id },
                { "ExistingTriggers", sessionTemplate.Triggers },
                { "Original", trigger },
                { "Modified", trigger.Clone() as CronSessionTrigger }
            };
            var dialog = DialogService.Show<EditCronTrigger>("Edit Trigger", parameters, new DialogOptions() { MaxWidth = MaxWidth.ExtraLarge, CloseButton = true });
            var result = await dialog.Result;
            if (!result.Canceled && result.Data is SessionTrigger modified)
            {
                sessionTemplate.Triggers.Remove(trigger);
                sessionTemplate.Triggers.Add(modified);
                SnackBar.Add($"Trigger was updated.", Severity.Success);
            }
        }


        /// <summary>
        /// Delete an existing trigger from the template
        /// </summary>
        /// <param name="claim"></param>
        /// <returns></returns>
        async Task RemoveTriggerAsync(SessionTrigger trigger)
        {
            if (sessionTemplate.Triggers.Contains(trigger))
            {
                var result = await TriggerService.DeleteTriggerAsync(sessionTemplate.Id, trigger);
                if (result.IsSuccess)
                {
                    sessionTemplate.Triggers.Remove(trigger);
                    SnackBar.Add($"Trigger was removed.", Severity.Success);
                    return;
                }
                SnackBar.Add(result.ToString(), Severity.Error);
            }
        }
      
        /// <summary>
        /// Pause all the triggers for the template
        /// </summary>
        /// <returns></returns>
        async Task PauseAllAsync()
        {
            var result = await TriggerService.PauseTemplateAsync(sessionTemplate.Name);
            if(result.IsSuccess)
            {
                SnackBar.Add($"All triggers were paused.", Severity.Success);
                this.sessionTemplate = await GetTemplateAsync(this.TemplateId);
                return;
            }
            SnackBar.Add(result.ToString(), Severity.Error);
        }

        /// <summary>
        /// Resume all the triggers for the template
        /// </summary>
        /// <returns></returns>
        async Task ResumeAllAsync()
        {
            var result = await TriggerService.ResumeTemplateAsync(sessionTemplate.Name);
            if (result.IsSuccess)
            {
                SnackBar.Add($"All triggers were resumed.", Severity.Success);
                this.sessionTemplate = await GetTemplateAsync(this.TemplateId);
                return;
            }
            SnackBar.Add(result.ToString(), Severity.Error);
        }

        /// <summary>
        /// Pause a specified trigger
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        async Task PauseTriggerAsync(SessionTrigger trigger)
        {
            if (sessionTemplate.Triggers.Contains(trigger) && trigger.IsEnabled)
            {
                var result = await TriggerService.PauseTriggerAsync(sessionTemplate.Name, trigger.Name);
                if (result.IsSuccess)
                {
                    trigger.IsEnabled = false;
                    SnackBar.Add($"Trigger : {trigger.Name} was paused.", Severity.Success);
                    return;
                }
                SnackBar.Add(result.ToString(), Severity.Error);
            }
        }

        /// <summary>
        /// Resume a specified trigger
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        async Task ResumeTriggerAsync(SessionTrigger trigger)
        {
            if (sessionTemplate.Triggers.Contains(trigger) && !trigger.IsEnabled)
            {
                var result = await TriggerService.ResumeTriggerAsync(sessionTemplate.Name, trigger.Name);
                if (result.IsSuccess)
                {
                    trigger.IsEnabled = true;
                    SnackBar.Add($"Trigger : {trigger.Name} was resumed.", Severity.Success);
                    return;
                }
                SnackBar.Add(result.ToString(), Severity.Error);
            }
        }

        async Task ScheduleTriggerNowAsync(SessionTrigger trigger)
        {
            if (sessionTemplate.Triggers.Contains(trigger))
            {
                var result = await TriggerService.ScheduleTriggerNowAsync(sessionTemplate.Id, trigger);
                if (result.IsSuccess)
                {                   
                    SnackBar.Add($"Trigger : {trigger.Name} was scheduled to execute now.", Severity.Success);
                    return;
                }
                SnackBar.Add(result.ToString(), Severity.Error);
            }
        }
    }
}
