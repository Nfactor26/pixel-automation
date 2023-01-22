using Pixel.Automation.Core.Attributes;
using Serilog;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Close the app-under-test on the device
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Close App", "Appium", "Device", iconSource: null, description: "Close the app-under-test on the device", tags: new string[] { "close", "app" })]
public class CloseAppActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<CloseAppActorComponent>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public CloseAppActorComponent() : base("Close App", "CloseApp")
    {

    }

    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;
        driver.CloseApp();
        logger.Information("Application under test was closed");
        await Task.CompletedTask;
    }
}
