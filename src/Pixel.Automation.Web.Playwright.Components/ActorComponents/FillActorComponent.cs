using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="FillActorComponent"/> fill out the form fields.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Fill", "Playwright", iconSource: null, description: "Fill out the form fields", tags: new string[] { "Fill", "Web" })]

public class FillActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<FillActorComponent>();

    /// <summary>
    /// Input argument to provide data to be filled on element
    /// </summary>
    [DataMember(IsRequired = true)]
    [Display(Name = "Input", GroupName = "Configuration", Order = 10, Description = "Input value to fill")]
    public Argument Input { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default };

    /// <summary>
    /// Optional input argument for <see cref="LocatorFillOptions"/> that can be used to customize the fill operation
    /// </summary>
    [DataMember]
    [Display(Name = "Offset", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorFillOptions")]
    public Argument FillOptions { get; set; } = new InArgument<LocatorFillOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public FillActorComponent() : base("Fill", "Fill")
    {

    }

    /// <summary>
    /// Fill input data on an input element using FillAsync() method
    /// </summary>
    public override async Task ActAsync()
    {
        var input = await this.ArgumentProcessor.GetValueAsync<string>(this.Input);
        var control = await GetTargetControl();
        await control.FillAsync(input, this.FillOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorFillOptions>(this.FillOptions) : null);
        logger.Information("Fill performed on element.");
    }
       
}
