using OpenQA.Selenium.Appium.iOS;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Perform a shake action on the device
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Shake", "Appium", "iOS", iconSource: null, description: "Perform a shake action on the device", tags: new string[] { "shake" })]
public class ShakeActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<ShakeActorComponent>();
   
    /// <summary>
    /// Default constructor
    /// </summary>
    public ShakeActorComponent() : base("Shake", "Shake")
    {

    }

    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;     
        if(driver is IOSDriver iOSDriver)
        {          
            iOSDriver.ShakeDevice();
        }        
        logger.Information("Device was shaked");
        await Task.CompletedTask;
    }
}
