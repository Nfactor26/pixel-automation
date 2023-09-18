using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Persistence.Core.Models;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Components.Tests;

/// <summary>
/// Component for showing the TraceData captured while execution of a test case
/// </summary>
public partial class TestTraces : ComponentBase
{
    [Inject]
    IDialogService DialogService { get; set; }

    [Inject]
    public ISnackbar SnackBar { get; set; }

    [Inject]
    ITestResultService TestResultService { get; set; }

    [Parameter]
    public TestResult TestResult { get; set; }   

    /// <summary>
    /// Get the color based on the trace level of the TraceData
    /// </summary>
    /// <param name="trace"></param>
    /// <returns></returns>
    Color GetColor(TraceData trace)
    {
        switch(trace.TraceLevel)
        {
            case Persistence.Core.Enums.TraceLevel.Information:
                return Color.Success;
            case Persistence.Core.Enums.TraceLevel.Warning:
                return Color.Warning;
            case Persistence.Core.Enums.TraceLevel.Error:
                return Color.Error;
            default:
                return Color.Default;
        }
    }

    /// <summary>
    /// Retreive image and show in a popup dialog on clicking the button
    /// </summary>
    /// <param name="traceData"></param>
    /// <returns></returns>
    async Task ShowImageAsync(ImageTraceData traceData)
    {
        try
        {
            var dataFile = await TestResultService.GetTraceImage(TestResult.Id, traceData.ImageFile);
            DialogOptions dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
            var parameters = new DialogParameters<ImageDialog> { { x => x.imageBytes, dataFile.Bytes } };
            var dialog = await DialogService.ShowAsync<ImageDialog>("Screenshot", parameters, dialogOptions);
            await dialog.Result;
        }
        catch (Exception ex)
        {
            SnackBar.Add(ex.Message, Severity.Error);
        }
    }
}
