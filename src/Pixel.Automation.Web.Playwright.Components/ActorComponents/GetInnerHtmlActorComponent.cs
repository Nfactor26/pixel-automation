using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="GetInnerHtmlActorComponent"/> to get inner html of an element.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get Inner Html", "Playwright", iconSource: null, description: "Get inner html of an element", tags: new string[] { "inner html", "inner", "html", "Web" })]

public class GetInnerHtmlActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetInnerHtmlActorComponent>();
        
    /// <summary>
    /// Optional input argument for <see cref="LocatorGetAttributeOptions"/> that can be used to customize the get inner html operation
    /// </summary>
    [DataMember]
    [Display(Name = "Get InnerHtml Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorInnerHTMLOptions")]
    public Argument GetInnerHtmlOptions { get; set; } = new InArgument<LocatorInnerHTMLOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Output argument to store the retrieved inner html of element
    /// </summary>
    [DataMember]
    [Display(Name = "Result", GroupName = "Output", Order = 10, Description = "Output argument to store the retrieved innerHtml value")]
    public Argument Result { get; set; } = new OutArgument<string>() { Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public GetInnerHtmlActorComponent() : base("Get Inner Html", "GetInnerHtml")
    {

    }

    /// <summary>
    /// Get inner html of an element using InnerHTMLAsync()
    /// </summary>
    public override async Task ActAsync()
    {
        var options = this.GetInnerHtmlOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorInnerHTMLOptions>(this.GetInnerHtmlOptions) : null;
        var (name, control) = await GetTargetControl();
        var result = await control.InnerHTMLAsync(options);
        await this.ArgumentProcessor.SetValueAsync<string>(this.Result, result);
        logger.Information("Retrieved innerHtml of control : '{0}'", name);
    }
}
