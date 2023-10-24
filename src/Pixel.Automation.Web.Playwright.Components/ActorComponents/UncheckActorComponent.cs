using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="UncheckActorComponent"/> to  uncheck a checkbox or a radio button.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Uncheck", "Playwright", iconSource: null, description: "UnCheck a checkbox or a radio button.", tags: new string[] { "check", "Web" })]

public class UncheckActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<UncheckActorComponent>();

    /// <summary>
    /// Optional input argument for <see cref="LocatorUncheckOptions"/> that can be used to customize the uncheck element operation
    /// </summary>
    [DataMember]
    [Display(Name = "UnCheck Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for LocatorUncheckOptions")]
    public Argument UnCheckOptions { get; set; } = new InArgument<LocatorUncheckOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };


    /// <summary>
    /// Constructor
    /// </summary>
    public UncheckActorComponent() : base("Uncheck", "Uncheck")
    {

    }

    /// <summary>
    /// Uncheck a checkbox or a radio button using UncheckAsync() method.
    /// A check is performed on element if the element already has a unchecked state before attempting to set it's state to unchecked.
    /// </summary>
    public override async Task ActAsync()
    {
        var (name, control) = await GetTargetControl();
        if (await control.IsCheckedAsync())
        {
            await control.UncheckAsync(this.UnCheckOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorUncheckOptions>(this.UnCheckOptions) : null);
            logger.Information("Control : '{0}' was unchecked", name);
        }
        logger.Information("Control : '{0}' is already unchecked", name);
    }

}
