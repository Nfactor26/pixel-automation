using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="WaitForActorComponent"/> to scroll element into view, unless it is completely visible
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Wait For", "Playwright", iconSource: null, description: "Wait for control to satisfy specified state", tags: new string[] { "Scroll", "Web" })]
public class WaitForActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<WaitForActorComponent>();

    /// <summary>
    /// Optional input argument for <see cref="LocatorScrollIntoViewIfNeededOptions"/> that can be used to customize the click operation on element
    /// </summary>
    [DataMember]
    [Display(Name = "Wait For Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for LocatorWaitForOptions")]
    public Argument WaitForOptions { get; set; } = new InArgument<LocatorWaitForOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public WaitForActorComponent() : base("Wait For", "WaitFor")
    {

    }

    /// <summary>
    /// Scroll element into view, unless it is completely visible 
    /// </summary>
    public override async Task ActAsync()
    {
        var (name, control) = await GetTargetControl();
        var options = this.WaitForOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorWaitForOptions>(this.WaitForOptions) : null;
        await control.WaitForAsync(options);
        logger.Information("Wait for control : '{0}' completed", name);
    }

}
