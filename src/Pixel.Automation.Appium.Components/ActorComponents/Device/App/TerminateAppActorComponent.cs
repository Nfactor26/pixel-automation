using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Terminate the given app onto the device
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Terminate App", "Appium", "Device", iconSource: null, description: "Terminate the given app onto the device", tags: new string[] { "terminate", "app" })]
public class TerminateAppActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<TerminateAppActorComponent>();

    [DataMember]
    [Display(Name = "App ID", GroupName = "Input", Order = 10, Description = "ID of app to Terminate")]
    public Argument AppID { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, CanChangeType = false, DefaultValue = string.Empty };

    [DataMember]
    [Display(Name = "Background Time", GroupName = "Input", Order = 20, Description = "Time to wait before application is terminated")]
    public Argument TimepSpan { get; set; } = new InArgument<TimeSpan>() { CanChangeType = false, DefaultValue = TimeSpan.FromSeconds(1) };

    /// <summary>
    /// Default constructor
    /// </summary>
    public TerminateAppActorComponent() : base("Terminate App", "TerminateApp")
    {

    }

    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;
        var appId = await this.ArgumentProcessor.GetValueAsync<string>(this.AppID);
        var time = await this.ArgumentProcessor.GetValueAsync<TimeSpan>(this.TimepSpan);
        driver.TerminateApp(appId, time);
        logger.Information("App : {0} was terminated after {1} seconds", appId, time.TotalSeconds);
    }
}
