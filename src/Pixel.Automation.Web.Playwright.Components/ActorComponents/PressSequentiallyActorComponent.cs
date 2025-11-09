using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="PressSequentiallyActorComponent"/> to type in to text field character by character.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Press Sequentially", "Playwright", iconSource: null, description: "Press keys one by one", tags: new string[] { "Type", "Web" })]

public class PressSequentiallyActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<PressSequentiallyActorComponent>();

    /// <summary>
    /// Input argument to provide the text to be pressed sequentially
    /// </summary>
    [DataMember(IsRequired = true)]
    [Display(Name = "Input", GroupName = "Configuration", Order = 10, Description = "Input argument for text to type")]
    public Argument Input { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default };


    /// <summary>
    ///  Optional input argument for <see cref="LocatorTypeOptions"/> that can be used to customize the type oepration
    /// </summary>
    [DataMember]
    [Display(Name = "Press Sequentially Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorPressSequentiallyOptions")]
    public Argument PressSequentiallyOptions { get; set; } = new InArgument<LocatorPressSequentiallyOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };


    /// <summary>
    /// Default constructor
    /// </summary>
    public PressSequentiallyActorComponent() : base("Press Sequentially", "PressSequentially")
    {

    }

    /// <summary>
    /// Type in to text field character by character using TypeAsync() method
    /// </summary>
    public override async Task ActAsync()
    {
        var input = await this.ArgumentProcessor.GetValueAsync<string>(this.Input);
        var (name, control) = await GetTargetControl();
        await control.PressSequentiallyAsync(input, this.PressSequentiallyOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorPressSequentiallyOptions>(this.PressSequentiallyOptions) : null);
        logger.Information("Type performed on element.");
    }

}
