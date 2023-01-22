using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Rotate device
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Rotate", "Appium", "Device", iconSource: null, description: "Rotate Device", tags: new string[] { "rotate", "device"})]
public class RotateActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<RotateActorComponent>();

    [DataMember]
    [Display(Name = "Rotate Options", GroupName = "Input", Order = 10, Description = "Parameter to rotate device e.g. {{\"x\", 114}, {\"y\", 198}, {\"duration\", 5}, {\"radius\", 3}, {\"rotation\", 220}, {\"touchCount\", 2}} ")]
    public Argument RotateOptions { get; set; } = new InArgument<Dictionary<string,int>>() { Mode = ArgumentMode.DataBound, CanChangeType = false, AllowedModes = ArgumentMode.DataBound|ArgumentMode.Scripted};

    /// <summary>
    /// Default constructor
    /// </summary>
    public RotateActorComponent() : base("Rotate", "Rotate")
    {

    }

    /// <summary>
    /// Rotate device
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;
        var options = await this.ArgumentProcessor.GetValueAsync<Dictionary<string,int>>(this.RotateOptions);
        driver.Rotate(options);
        logger.Information("Device was rotated");      
    }
}
