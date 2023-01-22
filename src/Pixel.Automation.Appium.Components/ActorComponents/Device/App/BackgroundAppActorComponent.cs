using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Send the currently running app for this session to the background
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Background App", "Appium", "Device", iconSource: null, description: "Send the currently running app for this session to the background", tags: new string[] { "background", "app" })]
public class BackgroundAppActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<BackgroundAppActorComponent>();

    [DataMember]
    [Display(Name = "Background Time", GroupName = "Input", Order = 10, Description = "Number of seconds to keep app in background. Negative value will deactivate the app completely")]
    public Argument TimepSpan { get; set; } = new InArgument<TimeSpan>() { CanChangeType = false, DefaultValue = TimeSpan.FromSeconds(-1) };

    /// <summary>
    /// Default constructor
    /// </summary>
    public BackgroundAppActorComponent() : base("Background App", "BackgroundApp")
    {

    }

    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;
        var time = await this.ArgumentProcessor.GetValueAsync<TimeSpan>(this.TimepSpan);
        driver.BackgroundApp(time);
        logger.Information("Application was sent to background for {0} seconds", time.TotalSeconds);
    }
}
