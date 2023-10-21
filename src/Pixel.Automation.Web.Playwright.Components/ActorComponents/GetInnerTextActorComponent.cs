using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="GetInnerTextActorComponent"/> to get inner text of an element.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get Inner Text", "Playwright", iconSource: null, description: "Get inner text of an element", tags: new string[] { "inner text", "inner", "text", "Web" })]

public class GetInnerTextActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetInnerTextActorComponent>();

    /// <summary>
    /// Optional input argument for <see cref="LocatorInnerTextOptions"/> that can be used to customize the get inner text operation
    /// </summary>
    [DataMember]
    [Display(Name = "Get InnerText Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorInnerTextOptions")]
    public Argument GetInnerTextOptions { get; set; } = new InArgument<LocatorInnerTextOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Output argument to store the attribute value of element
    /// </summary>
    [DataMember]
    [Display(Name = "Result", GroupName = "Output", Order = 10, Description = "Output argument to store the retrieved innerText value")]
    public Argument Result { get; set; } = new OutArgument<string>() { Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public GetInnerTextActorComponent() : base("Get Inner Text", "GetInnerText")
    {

    }

    /// <summary>
    /// Get inner text of an element using InnerTextAsync()
    /// </summary>
    public override async Task ActAsync()
    {
        var options = this.GetInnerTextOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorInnerTextOptions>(this.GetInnerTextOptions) : null;
        var (name, control) = await GetTargetControl();
        var result = await control.InnerTextAsync(options);
        await this.ArgumentProcessor.SetValueAsync<string>(this.Result, result);
        logger.Information("Retrieved innerText of control : '{0}'", name);
    }
}
