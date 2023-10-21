using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ClickActorComponent"/> to scroll element into view, unless it is completely visible
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Scroll into view", "Playwright", iconSource: null, description: "Scroll element into view, unless it is completely visible", tags: new string[] { "Scroll", "Web" })]
public class ScrollIntoViewActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<ScrollIntoViewActorComponent>();

    /// <summary>
    /// Optional input argument for <see cref="LocatorScrollIntoViewIfNeededOptions"/> that can be used to customize the click operation on element
    /// </summary>
    [DataMember]
    [Display(Name = "Scroll Into View Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for LocatorScrollIntoViewIfNeededOptions")]
    public Argument ScrollIntoViewOptions { get; set; } = new InArgument<LocatorScrollIntoViewIfNeededOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ScrollIntoViewActorComponent() : base("Scroll Into View", "ScrollIntoView")
    {

    }

    /// <summary>
    /// Scroll element into view, unless it is completely visible 
    /// </summary>
    public override async Task ActAsync()
    {
        var (name, control) = await GetTargetControl();
        var options = this.ScrollIntoViewOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorScrollIntoViewIfNeededOptions>(this.ScrollIntoViewOptions) : null;
        await control.ScrollIntoViewIfNeededAsync(options);
        logger.Information("Control : '{0}' was scrolled into view", name);
    }

}
