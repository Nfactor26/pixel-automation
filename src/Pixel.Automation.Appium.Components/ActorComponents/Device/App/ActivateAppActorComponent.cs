using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Activate the given app onto the device
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Activate App", "Appium", "Device", iconSource: null, description: "Activate the given app onto the device", tags: new string[] { "activate", "app" })]
public class ActivateAppActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<ActivateAppActorComponent>();

    [DataMember]
    [Display(Name = "App ID", GroupName = "Input", Order = 10, Description = "ID of app to activate")]
    public Argument AppID { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, CanChangeType = false, DefaultValue = string.Empty };

    /// <summary>
    /// Default constructor
    /// </summary>
    public ActivateAppActorComponent() : base("Activate App", "ActivateApp")
    {

    }

    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;
        var appId = await this.ArgumentProcessor.GetValueAsync<string>(this.AppID);
        driver.ActivateApp(appId);
        logger.Information("App : {0} was activated", appId);
    }
}
