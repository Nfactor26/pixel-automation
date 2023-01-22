using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Get the current device/browser orientation
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get Orientation", "Appium", "Session", iconSource: null, description: "Get the current device/browser orientation", tags: new string[] { "orientation", "get orientation" })]
public class GetOrientationActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetOrientationActorComponent>();

    /// <summary>
    /// Set the text on a web control. To send special keys such as Enter however, use scripted mode and return desired keys. For Example : Keys.Control + \"A\" or Keys.Enter etc.
    /// </summary>
    [DataMember]
    [Display(Name = "Orientation", GroupName = "Output", Order = 10, Description = "Orientation of current device/browser")]
    public Argument Orientation { get; set; } = new OutArgument<ScreenOrientation>() { CanChangeType = false };


    /// <summary>
    /// Default constructor
    /// </summary>
    public GetOrientationActorComponent() : base("Get Orientation", "GetOrientation")
    {

    }

    /// <summary>
    /// Get the current device/browser orientation
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;
        await this.ArgumentProcessor.SetValueAsync<ScreenOrientation>(this.Orientation, driver.Orientation);
        logger.Information("Current device orientation is {0}", driver.Orientation);       
    }
}
