using Microsoft.AspNetCore.Components;

namespace Pixel.Automation.Web.Portal.Pages.Tests;

public partial class StatisticsPage : ComponentBase
{      
    [Parameter]
    public string TestId { get; set; }     
}
