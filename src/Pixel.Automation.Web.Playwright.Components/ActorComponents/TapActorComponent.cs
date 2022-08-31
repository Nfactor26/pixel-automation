using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="TapActorComponent"/> to perform a tap on element.  
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Tap", "Playwright", iconSource: null, description: "Perform a tap action", tags: new string[] { "Tap", "Web" })]
public class TapActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<TapActorComponent>();

    /// <summary>
    ///  Optional input argument for <see cref="LocatorTapOptions"/> that can be used to customize the tap element operation
    /// </summary>
    [DataMember]
    [Display(Name = "Tap Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorTapOptions")]
    public Argument TapOptions { get; set; } = new InArgument<LocatorTapOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Cosntructor
    /// </summary>
    public TapActorComponent() : base("Tap", "Tap")
    {

    }

    /// <summary>
    /// Perform a tap on an element using TapAsync() method.  
    /// </summary>
    public override async Task ActAsync()
    {
        var control = await GetTargetControl();
        await control.TapAsync(this.TapOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorTapOptions>(this.TapOptions) : null);
        logger.Information("control was tapped.");
    }
   
}
