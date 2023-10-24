using Pixel.Automation.Core.Attributes;
using Serilog;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Use <see cref="GoForwardActorComponent"/> to navigate to the next page in history.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Go Forward", "Appium", "Browser", iconSource: null, description: "Navigate to the next page in history", tags: new string[] { "forward", "go" })]
public class GoForwardActorComponent : AppiumElementActorComponent
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
        this.ApplicationDetails.Driver.Navigate().Forward();
        logger.Information("Browser was navigated to the next page : '{0}' in history", this.ApplicationDetails.Driver.Url);
        await Task.CompletedTask;
    }
}
