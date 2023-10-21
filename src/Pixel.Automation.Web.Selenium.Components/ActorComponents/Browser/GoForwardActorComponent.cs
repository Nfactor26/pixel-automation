using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Web.Selenium.Components;
using Serilog;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="GoForwardActorComponent"/> to navigate to the next page in history.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Go Forward", "Selenium", "Browser", iconSource: null, description: "Navigate to the next page in history", tags: new string[] { "GoForward", "Forward", "Navigate", "Browser", "Web" })]
public class GoForwardActorComponent : WebElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<GoForwardActorComponent>();

    /// <summary>
    /// Constructor
    /// </summary>
    public GoForwardActorComponent():base("Go Forward", "GoForward")
    {

    }

    /// <summary>
    /// Navigate to the next page in history
    /// </summary>
    public override async Task ActAsync()
    {
        this.ApplicationDetails.WebDriver.Navigate().Forward();
        logger.Information("Browser was navigated to the next page : '{0}' in history", this.ApplicationDetails.WebDriver.Url);
        await Task.CompletedTask;
    }
}
