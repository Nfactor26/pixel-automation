using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Unlock the device
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Unlock", "Appium", "Device", iconSource: null, description: "Unlock the device", tags: new string[] { "unlock" })]
public class UnlockActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<UnlockActorComponent>();
 
    /// <summary>
    /// Default constructor
    /// </summary>
    public UnlockActorComponent() : base("Unlock", "Unlock")
    {

    }

    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;     
        if(driver is AndroidDriver androidDriver)
        {
            androidDriver.Unlock();
        }
        else if(driver is IOSDriver iOSDriver)
        {           
            iOSDriver.Unlock();
        }        
        logger.Information("Deivce was unlocked");
        await Task.CompletedTask;
    }
}
