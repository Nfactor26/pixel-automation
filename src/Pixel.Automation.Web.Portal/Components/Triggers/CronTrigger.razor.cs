using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Components.Triggers
{
    public partial class CronTrigger : ComponentBase
    {
        string error = null;
        CronSessionTrigger model = new();

        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public ITriggerService Service { get; set; }

        [Parameter]
        public string TemplateId { get; set; }

        [Parameter]
        public IEnumerable<SessionTrigger> ExistingTriggers { get; set; }

        /// <summary>
        /// Add new trigger and close the dialog
        /// </summary>
        async Task AddNewTriggerAsync()
        {
            if (!ExistingTriggers.Any(t => t.Equals(model)))
            {
                //Don't try to add claim to role if role is not yet created.
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
            error = $"Trigger with same details already exists for Template.";
        }

        /// <summary>
        /// Close the dialog without any result
        /// </summary>
        void Cancel() => MudDialog.Cancel();
    }
}
