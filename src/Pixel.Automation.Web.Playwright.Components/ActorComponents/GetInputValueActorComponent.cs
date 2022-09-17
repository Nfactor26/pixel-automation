using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="GetInputValueActorComponent"/> to retieve the value of selected <input> or <select> or <textarea>.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get Input Value", "Playwright", iconSource: null, description: "Get input value of an element", tags: new string[] { "input value", "value", "Web" })]

public class GetInputValueActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetInputValueActorComponent>();

    /// <summary>
    /// Optional input argument for <see cref="LocatorInputValueOptions"/> that can be used to customize the get input value operation
    /// </summary>
    [DataMember]
    [Display(Name = "Get InputValue Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorInputValueOptions")]
    public Argument GetInputvalueOptions { get; set; } = new InArgument<LocatorInputValueOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Output argument to store the attribute value of element
    /// </summary>
    [DataMember]
    [Display(Name = "Result", GroupName = "Output", Order = 10, Description = "Output argument to store the retrieved input value value")]
    public Argument Result { get; set; } = new OutArgument<string>() { Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public GetInputValueActorComponent() : base("Get Input Value", "GetInputValue")
    {

    }

    /// <summary>
    /// Retieve the value of selected <input> or <select> or <textarea> using InputValueAsync()
    /// </summary>
    public override async Task ActAsync()
    {     
        var options = this.GetInputvalueOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorInputValueOptions>(this.GetInputvalueOptions) : null;
        var control = await GetTargetControl();
        var result = await control.InputValueAsync(options);
        await this.ArgumentProcessor.SetValueAsync<string>(this.Result, result);
        logger.Information($"Retrieved input value of element.");
    }

}
