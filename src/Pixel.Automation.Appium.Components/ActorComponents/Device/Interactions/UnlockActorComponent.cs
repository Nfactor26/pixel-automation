using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
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

    [DataMember]
    [Display(Name = "Key", GroupName = "Input", Order = 10, Description = "[Android] The unlock key. See the documentation on appium:unlockKey capability for more details.")]
    public Argument Key { get; set; } = new InArgument<string>() { CanChangeType = false, Mode = ArgumentMode.Default };

    [DataMember]
    [Display(Name = "Type", GroupName = "Input", Order = 20, Description = "[Android] The unlock type. See the documentation on appium:unlockType capability for more details.")]
    public Argument Type { get; set; } = new InArgument<string>() { CanChangeType = false, Mode = ArgumentMode.Default };

    [DataMember]
    [Display(Name = "Strategy", GroupName = "Input", Order = 20, Description = "[Android] Optional unlock strategy. See the documentation on appium:unlockStrategy capability for more details.")]
    public Argument Strategy { get; set; } = new InArgument<string?>() { CanChangeType = false, Mode = ArgumentMode.Default };

    [DataMember]
    [Display(Name = "Timeout", GroupName = "Input", Order = 20, Description = "[Android] Optional unlock timeout in milliseconds. See the documentation on appium:unlockSuccessTimeout capability for more details.")]
    public Argument Timeout { get; set; } = new InArgument<int?>() { CanChangeType = false, Mode = ArgumentMode.Default };

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
            string key = await this.ArgumentProcessor.GetValueAsync<string>(this.Key);
            string type = await this.ArgumentProcessor.GetValueAsync<string>(this.Type);
            string? strategy = this.Strategy.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<string?>(this.Strategy) : null;
            int? timeout = this.Timeout.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<int?>(this.Timeout) : null;
            androidDriver.Unlock(key, type, strategy, timeout);
        }
        else if(driver is IOSDriver iOSDriver)
        {           
            iOSDriver.Unlock();
        }        
        logger.Information("Deivce was unlocked");
        await Task.CompletedTask;
    }
}
