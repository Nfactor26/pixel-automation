using Pixel.Automation.Core.Attributes;
using Serilog;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// Use <see cref="GoBackActorComponent"/> to navigate the previous page in history.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Go Back", "Selenium", "Browser", iconSource: null, description: "Navigate to the previous page in history", tags: new string[] { "GoBack", "Back", "Navigate", "Browser", "Web" })]
public class GoBackActorComponent : WebElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<GoBackActorComponent>();

    /// <summary>
    /// Constructor
    /// </summary>
    public GoBackActorComponent():base("Go Back", "GoBack")
    {

    }

    /// <summary>
    /// Navigate to the previous page in history
    /// </summary>
    public override async Task ActAsync()
    {
        this.ApplicationDetails.WebDriver.Navigate().Back();
        logger.Information("Browser was navigated to the previous page : '{0}' in history", this.ApplicationDetails.WebDriver.Url);
        await Task.CompletedTask;
    }
}
