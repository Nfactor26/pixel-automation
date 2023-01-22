using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Remove a given app from device
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Remove App", "Appium", "Device", iconSource: null, description: "Remove a given app from the device", tags: new string[] { "remove", "app" })]
public class RemoveAppActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<RemoveAppActorComponent>();

    [DataMember]
    [Display(Name = "App ID", GroupName = "Input", Order = 10, Description = "ID of app to remove")]
    public Argument AppID { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, CanChangeType = false, DefaultValue = string.Empty };

    /// <summary>
    /// Default constructor
    /// </summary>
    public RemoveAppActorComponent() : base("Remove App", "RemoveApp")
    {

    }

    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;
        var appId = await this.ArgumentProcessor.GetValueAsync<string>(this.AppID);
        driver.RemoveApp(appId);
        logger.Information("App : {0} was removed", appId);
    }
}
