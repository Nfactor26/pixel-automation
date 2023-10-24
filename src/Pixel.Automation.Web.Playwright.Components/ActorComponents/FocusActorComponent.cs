using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="FocusActorComponent"/> to focus on an element.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Focus", "Playwright", iconSource: null, description: "Focus on an element", tags: new string[] { "Focus", "Web" })]

public class FocusActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<FocusActorComponent>();

    /// <summary>
    /// Optional input argument for <see cref="LocatorFocusOptions"/> that can be used to customize the focus element operation
    /// </summary>
    [DataMember]
    [Display(Name = "Focus Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for LocatorFocusOptions")]
    public Argument FocusOptions { get; set; } = new InArgument<LocatorFocusOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public FocusActorComponent() : base("Focus", "Focus")
    {

    }

    /// <summary>
    /// Focus on an element using FocusAsync() method
    /// </summary>
    public override async Task ActAsync()
    {     
        var (name, control) = await GetTargetControl();
        await control.FocusAsync(this.FocusOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorFocusOptions>(this.FocusOptions) : null);
        logger.Information("Focus set on control : '{0}'", name);
    }

}
