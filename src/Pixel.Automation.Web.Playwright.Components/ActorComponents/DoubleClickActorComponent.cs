using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="DoubleClickActorComponent"/> to perform a double click on an element.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Double Click", "Playwright", iconSource: null, description: "Perform a double click on an element", tags: new string[] { "click", "double", "Web" })]

public class DoubleClickActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<DoubleClickActorComponent>();

    /// <summary>
    /// Optional input argument for <see cref="LocatorDblClickOptions"/> that can be used to customize the double click operation
    /// </summary>
    [DataMember]
    [Display(Name = "Double Click Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for LocatorDblClickOptions")]
    public Argument DblClickOptions { get; set; } = new InArgument<LocatorDblClickOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public DoubleClickActorComponent() : base("Double Click", "DoubleClick")
    {

    }

    /// <summary>
    /// Perform a double click on an elment using DblClickAsync() method
    /// </summary>
    public override async Task ActAsync()
    {
        var control = await GetTargetControl();
        await control.DblClickAsync(this.DblClickOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorDblClickOptions>(this.DblClickOptions) : null);
        logger.Information("control was double clicked.");
    }

    ///</inheritdoc>
    public override string ToString()
    {
        return "Double Click Actor";
    }
}
