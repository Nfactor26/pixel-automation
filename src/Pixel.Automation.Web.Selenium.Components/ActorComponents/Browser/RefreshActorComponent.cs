using Pixel.Automation.Core.Attributes;
using Serilog;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// Use <see cref="RefreshActorComponent"/> to refresh the page.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Refresh", "Selenium", "Browser", iconSource: null, description: "Refresh active page", tags: new string[] { "Refresh", "Navigate", "Browser", "Web" })]
public class RefreshActorComponent : WebElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<RefreshActorComponent>();

    /// <summary>
    /// Constructor
    /// </summary>
    public RefreshActorComponent():base("Refresh", "Refresh")
    {

    }

    /// <summary>
    /// Refresh the web page
    /// </summary>
    public override async Task ActAsync()
    {
        this.ApplicationDetails.WebDriver.Navigate().Refresh();
        logger.Information("Browser window was refreshed");
        await Task.CompletedTask;
    }
}
