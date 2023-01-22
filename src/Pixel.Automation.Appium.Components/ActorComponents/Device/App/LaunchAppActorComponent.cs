using Pixel.Automation.Core.Attributes;
using Serilog;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Launch the app-under-test on the device
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Launch App", "Appium", "Device", iconSource: null, description: "Launch the app-under-test on the device", tags: new string[] { "launch", "app" })]
public class LaunchAppActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<LaunchAppActorComponent>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public LaunchAppActorComponent() : base("Launch App", "LaunchApp")
    {

    }

    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;     
        driver.LaunchApp();
        logger.Information("Application under test was launched");
        await Task.CompletedTask;
    }
}
