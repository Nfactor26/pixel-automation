using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Lock the device
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Lock", "Appium", "Device", iconSource: null, description: "Lock the device", tags: new string[] { "lock" })]
public class LockActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<LockActorComponent>();

    [DataMember]
    [Display(Name = "Time to lock", GroupName = "Input", Order = 10, Description = "Number of seconds to keep the screen locked. IOS Only.")]   
    public Argument TimeToLock { get; set; } = new InArgument<int>() { CanChangeType = false, DefaultValue = 0 };

    /// <summary>
    /// Default constructor
    /// </summary>
    public LockActorComponent() : base("Lock", "Lock")
    {

    }

    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;     
        if(driver is AndroidDriver androidDriver)
        {
            androidDriver.Lock();
        }
        else if(driver is IOSDriver iOSDriver)
        {
            var timeToLock = await this.ArgumentProcessor.GetValueAsync<int>(this.TimeToLock);
            iOSDriver.Lock(timeToLock);
        }        
        logger.Information("Deivce was locked");
        await Task.CompletedTask;
    }
}
