using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Pages.Handlers;

public partial class AddHandler : ComponentBase
{
    [Inject]
    public IDialogService DialogService { get; set; }

    [Inject]
    public ISnackbar SnackBar { get; set; }     

    [Inject]
    public IHandlerService HandlersService { get; set; }

    [Inject]
    public IComposeFileService ComposeFileService { get; set; }

    [Inject]
    public NavigationManager Navigator { get; set; }

    string[] environments = new[] { "Docker", "Linux", "Windows" };
    
    string environment = "Docker";
    string Environment
    {
        get => environment; 
        set
        {
            if(environment != value)
            {
                environment = value;
                switch(value)
                {
                    case "Docker":
                        templateHandler = new DockerTemplateHandler();
                        break;
                    case "Windows":
                        templateHandler = new WindowsTemplateHandler();
                        break;
                    case "Linux":
                        templateHandler = new LinuxTemplateHandler();
                        break;
                }
            }
        }
    }

    List<string> dockerComposeFiles = new();

    TemplateHandler templateHandler = new DockerTemplateHandler();

    string parameters = string.Empty;


    protected override async Task OnInitializedAsync()
    {
        dockerComposeFiles.Clear();
        var availableComposeFiles = await ComposeFileService.GetComposeFileNamesAsync();
        dockerComposeFiles.AddRange(availableComposeFiles);
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Add new template handler
    /// </summary>
    /// <returns></returns>
    private async Task AddHandlerAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(parameters))
            {
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
            var result = await HandlersService.AddHandlerAsync(this.templateHandler);
            if (result.IsSuccess)
            {
                Navigator.NavigateTo($"handlers/list");
                SnackBar.Add("Added successfully.", Severity.Success);
                return;
            }
            SnackBar.Add(result.ToString(), Severity.Error);
        }
        catch (Exception ex)
        {
            SnackBar.Add(ex.Message, Severity.Error);
        }
    }


    static string DefaultDragClass = "relative rounded-lg border-2 border-dashed pa-4 mt-4 mud-width-full mud-height-full z-10";
    string DragClass = DefaultDragClass;
    List<IBrowserFile> filesToUpload = new();

    void OnInputFileChanged(InputFileChangeEventArgs e)
    {
        ClearDragClass();
        filesToUpload.Clear();
        filesToUpload.AddRange(e.GetMultipleFiles());      
    }

    private async Task Clear()
    {
        filesToUpload.Clear();
        ClearDragClass();
        await Task.Delay(100);
    }
    private async Task Upload()
    {
        var existingFiles = filesToUpload.Select(s => s.Name).Intersect(dockerComposeFiles);
        if (existingFiles.Count() > 0)
        {
            bool? dialogResult = await DialogService.ShowMessageBox("Warning", $"Files {string.Join(',', existingFiles)} already exist.",
               yesText: "Replace", cancelText: "Cancel", options: new DialogOptions() { FullWidth = true });
            if (!dialogResult.GetValueOrDefault())
            {
                return;
            }
        }
        foreach (var file in filesToUpload)
        {          
            var result = await ComposeFileService.AddComposeFileAsync(file);
            if (result.IsSuccess)
            {
                SnackBar.Add($" {file.Name} was uploaded successfully.", Severity.Success);
                continue;
            }
            SnackBar.Add(result.ToString(), Severity.Error);
        }
        dockerComposeFiles.Clear();
        var availableComposeFiles = await ComposeFileService.GetComposeFileNamesAsync();
        dockerComposeFiles.AddRange(availableComposeFiles);
    }

    private void SetDragClass()
    {
        DragClass = $"{DefaultDragClass} mud-border-primary";
    }

    private void ClearDragClass()
    {
        DragClass = DefaultDragClass;
    }
}
