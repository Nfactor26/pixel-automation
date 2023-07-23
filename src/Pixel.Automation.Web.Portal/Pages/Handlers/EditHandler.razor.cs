using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Pages.Handlers;

public partial class EditHandler : ComponentBase
{
    [Parameter]
    public string HandlerId { get; set; }

    [Inject]
    public IDialogService DialogService { get; set; }

    [Inject]
    public IComposeFileService ComposeFileService { get; set; }

    [Inject]
    public ISnackbar SnackBar { get; set; }

    [Inject]
    public IHandlerService HandlersService { get; set; }

    [Inject]
    public NavigationManager Navigator { get; set; }

    List<string> dockerComposeFiles = new();

    TemplateHandler templateHandler;
    string parameters = string.Empty;


    protected override async Task OnInitializedAsync()
    {
        templateHandler = await HandlersService.GetHandlerByIdAsync(HandlerId);
        if(templateHandler is DockerTemplateHandler)
        {
            dockerComposeFiles.Clear();
            var availableComposeFiles = await ComposeFileService.GetComposeFileNamesAsync();
            dockerComposeFiles.AddRange(availableComposeFiles);
        }
        StringBuilder sb = new StringBuilder();
        int i = 0;
        foreach(var argument in templateHandler.Parameters)
        {
            sb.Append(argument.Key);
            sb.Append("=");
            sb.Append(argument.Value);
            i++;
            if(i < templateHandler.Parameters.Count)
            {
                sb.Append(',');
            }
        }
        parameters = sb.ToString();
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Add new template handler
    /// </summary>
    /// <returns></returns>
    private async Task UpdateHandlerAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(parameters))
            {
                this.templateHandler.Parameters.Clear();
                foreach (var arg in parameters.Split(','))
                {
                    var keyValuePair = arg.Split('=');
                    if (keyValuePair.Length != 2)
                    {
                        throw new ArgumentException($"Argument {arg} could not be parsed.");
                    }
                    this.templateHandler.Parameters.Add(keyValuePair[0], keyValuePair[1]);
                }
            }
            var result = await HandlersService.UpdateHandlerAsync(this.templateHandler);
            if (result.IsSuccess)
            {
                SnackBar.Add("Updated successfully.", Severity.Success);
                return;
            }
            SnackBar.Add(result.ToString(), Severity.Error);

        }       
        catch(Exception ex)
        {
            SnackBar.Add(ex.Message, Severity.Error);
        }
    }

}