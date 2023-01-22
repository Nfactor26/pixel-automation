using OpenQA.Selenium;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Set the current device/browser orientation
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Set Orientation", "Appium", "Session", iconSource: null, description: "Set the current device/browser orientation", tags: new string[] { "orientation", "set orientation" })]
public class SetOrientationActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<SetOrientationActorComponent>();

    /// <summary>
    /// ScreenOrientation to set
    /// </summary>
    [DataMember]
    [Display(Name = "Orientation", GroupName = "Input", Order = 10, Description = "Orientation of current device/browser")]
    public ScreenOrientation Orientation { get; set; } = ScreenOrientation.Landscape;


    /// <summary>
    /// Default constructor
    /// </summary>
    public SetOrientationActorComponent() : base("Set Orientation", "SetOrientation")
    {      
        
    }

    /// <summary>
    /// Set the current device/browser orientation
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;
        driver.Orientation = this.Orientation;
        logger.Information("Device orientation was changed to {0}", Orientation);
        await Task.CompletedTask;
    }
}
