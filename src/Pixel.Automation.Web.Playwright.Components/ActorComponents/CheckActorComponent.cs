using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="CheckActorComponent"/> to check a checkbox or a radio button.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Check", "Playwright", iconSource: null, description: "Check a checkbox or a radio button", tags: new string[] { "check", "Web" })]

public class CheckActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<CheckActorComponent>();

    /// <summary>
    /// Optional input argument for <see cref="LocatorCheckOptions"/> that can be used to customize the check element operation
    /// </summary>
    [DataMember]
    [Display(Name = "Check Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for LocatorCheckOptions")]
    public Argument CheckOptions { get; set; } = new InArgument<LocatorCheckOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

   
    /// <summary>
    /// Constructor
    /// </summary>
    public CheckActorComponent() : base("Check", "Check")
    {

    }

    /// <summary>
    /// Check a checkbox or a radio button using CheckAsync() method.
    /// A check is performed on element if the element already has a checked state before attempting to set it's state to checked.
    /// </summary>
    public override async Task ActAsync()
    {
        var (name,control) = await GetTargetControl();
        if(!await control.IsCheckedAsync())
        {
            await control.CheckAsync(this.CheckOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorCheckOptions>(this.CheckOptions) : null);
            logger.Information("Control : '{0}' was checked", name);
        }
        logger.Warning("Control : '{0}' is already checked", name);
    }
}
