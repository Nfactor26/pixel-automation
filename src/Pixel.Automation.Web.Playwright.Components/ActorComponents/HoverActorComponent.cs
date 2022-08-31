using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="HoverActorComponent"/> to simulate hover on an element.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Hover", "Playwright", iconSource: null, description: "Perform a hover action on an element", tags: new string[] { "Hover", "Web" })]

public class HoverActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<HoverActorComponent>();

    /// <summary>
    /// Optional input argument for <see cref="LocatorHoverOptions"/> that can be used to customize the hover operation
    /// </summary>
    [DataMember]
    [Display(Name = "Hover Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for LocatorHoverOptions")]      
    public Argument HoverOptions { get; set; } = new InArgument<LocatorHoverOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public HoverActorComponent() : base("Hover", "Hover")
    {

    }

    /// <summary>
    /// Hover over element using HoverAsync() method
    /// </summary>
    public override async Task ActAsync()
    {       
        var control = await GetTargetControl();
        await control.HoverAsync(this.HoverOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorHoverOptions>(this.HoverOptions) : null);
        logger.Information("Hover over element done.");
    }

}
