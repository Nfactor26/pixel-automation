using Pixel.Automation.Core.Attributes;
using Serilog;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Reset the currently running app for this session
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Reset App", "Appium", "Device", iconSource: null, description: "Reset the currently running app for this session", tags: new string[] { "reset", "app" })]
public class ResetAppActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<ResetAppActorComponent>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public ResetAppActorComponent() : base("Reset App", "ResetApp")
    {

    }

    /// <summary>
    /// Reset the currently running app for this session
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;     
        driver.ResetApp();
        logger.Information("App was reset");
        await Task.CompletedTask;
    }
}
