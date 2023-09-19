using Microsoft.AspNetCore.Components;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Persistence.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Pages.Tests;

/// <summary>
/// Page for viewing trace data captured while execution of a test case
/// </summary>
public partial class TracesPage : ComponentBase
{
    TestResult testResult;
    
    [Parameter]
    public string TestResultId { get; set; }
      
    [Inject]
    ITestResultService TestResultService { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrEmpty(TestResultId))
        {
            testResult = await TestResultService.GetResultById(TestResultId);           
        }
    }
}
