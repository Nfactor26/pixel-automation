using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pixel.Automation.Core;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Components.Triggers;

public partial class EditCronTrigger  :ComponentBase
{
    [Parameter]
    public CronSessionTrigger Original { get; set; }

    [Parameter]
    public CronSessionTrigger Modified { get; set; }

    [Parameter]
    public string TemplateId { get; set; }

    [Parameter]
    public IEnumerable<SessionTrigger> ExistingTriggers { get; set; }

    [Inject]
    public IHandlerService HandlersService { get; set; }

    [Inject]
    public ITriggerService TriggersService { get; set; }

    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; }

    List<TemplateHandler> templateHandlers = new();
    string parameters = string.Empty;
    string error = null;

    protected override async Task OnInitializedAsync()
    {
        templateHandlers.Clear();
        var availableHandlers = await HandlersService.GetAllAsync();
        templateHandlers.AddRange(availableHandlers);
        parameters = Modified.Parameters.ToCommaSeperateString();
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Add new trigger and close the dialog
    /// </summary>
    async Task UpdateTriggerAsync()
    {
        try
        {
            this.Modified.Parameters.Clear();
            foreach(var kv in parameters?.ToDictionary())
            {
                this.Modified.Parameters.Add(kv.Key, kv.Value);
            }           
            if (!string.IsNullOrEmpty(TemplateId))
            {
                var result = await TriggersService.UpdateTriggerAsync(TemplateId, Original, Modified);
                if (result.IsSuccess)
                {
                    MudDialog.Close(DialogResult.Ok<SessionTrigger>(Modified));
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

    /// <summary>
    /// Close the dialog without any result
    /// </summary>
    void Cancel() => MudDialog.Cancel();
}
