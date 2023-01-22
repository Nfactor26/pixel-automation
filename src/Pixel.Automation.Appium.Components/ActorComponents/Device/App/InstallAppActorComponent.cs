using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Install the given app onto the device
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Install App", "Appium", "Device", iconSource: null, description: "Install the given app onto the device", tags: new string[] { "intall", "app" })]
public class InstallAppActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<InstallAppActorComponent>();

    [DataMember]
    [Display(Name = "App Path", GroupName = "Input", Order = 10, Description = "File path or url of the app")]
    public Argument AppPath { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, CanChangeType = false, DefaultValue = string.Empty };

    /// <summary>
    /// Default constructor
    /// </summary>
    public InstallAppActorComponent() : base("Install App", "InstallApp")
    {

    }

    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;
        var appPath = await this.ArgumentProcessor.GetValueAsync<string>(this.AppPath);
        driver.InstallApp(appPath);
        logger.Information("App : {0} was installed", appPath);
    }
}
