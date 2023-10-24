using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ClickActorComponent"/> to perform a click on an element.  
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Click", "Playwright", iconSource: null, description: "Perform click on an element", tags: new string[] { "Click", "Web" })]
public class ClickActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<ClickActorComponent>();

    /// <summary>
    /// Optional input argument for <see cref="LocatorClickOptions"/> that can be used to customize the click operation on element
    /// </summary>
    [DataMember]
    [Display(Name = "Click Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for LocatorClickOptions")]
    public Argument ClickOptions { get; set; } = new InArgument<LocatorClickOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ClickActorComponent() : base("Click", "Click")
    {

    }

    /// <summary>
    /// Perform a click on on an element using ClickAsync() method.  
    /// </summary>
    public override async Task ActAsync()
    {
        var (name, control) = await GetTargetControl();
        await control.ClickAsync(this.ClickOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorClickOptions>(this.ClickOptions) : null);
        logger.Information("Control : '{0}' was clicked", name);
    }

}
