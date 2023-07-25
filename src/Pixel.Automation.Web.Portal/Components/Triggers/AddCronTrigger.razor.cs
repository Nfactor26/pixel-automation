using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pixel.Automation.Core;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Components.Triggers
{
    public partial class AddCronTrigger : ComponentBase
    {
        string error = null;
        CronSessionTrigger model = new();
        string parameters = string.Empty;

        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Inject]
        public IHandlerService HandlersService { get; set; }

        [Parameter]
        public ITriggerService Service { get; set; }

        [Parameter]
        public string TemplateId { get; set; }

        [Parameter]
        public IEnumerable<SessionTrigger> ExistingTriggers { get; set; }


        List<TemplateHandler> templateHandlers = new();


        protected override async Task OnInitializedAsync()
        {
            templateHandlers.Clear();
            var availableHandlers = await HandlersService.GetAllAsync();
            templateHandlers.AddRange(availableHandlers);
            await base.OnInitializedAsync();
        }

        /// <summary>
        /// Add new trigger and close the dialog
        /// </summary>
        async Task AddNewTriggerAsync()
        {
            if (!ExistingTriggers.Any(t => t.Equals(model)))
            {
                try
                {
                    model.Parameters.Clear();
                    foreach (var kv in parameters?.ToDictionary())
                    {
                        this.model.Parameters.Add(kv.Key, kv.Value);
                    }                  
                    if (!string.IsNullOrEmpty(TemplateId))
                    {
                        var result = await Service.AddTriggerAsync(TemplateId, model);
                        if (result.IsSuccess)
                        {
                            MudDialog.Close(DialogResult.Ok<SessionTrigger>(model));
                            return;
                        }
                        error = result.ToString();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    return;
                }   
            }
            error = $"Trigger with same details already exists for Template.";
        }

        /// <summary>
        /// Close the dialog without any result
        /// </summary>
        void Cancel() => MudDialog.Cancel();
    }
}
