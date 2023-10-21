using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="GetAttributeActorComponent"/> to get specified attribute of an element.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get Attribute", "Playwright", iconSource: null, description: "Get specified attribute of an element", tags: new string[] { "attribute", "Web" })]

public class GetAttributeActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetAttributeActorComponent>();

    [DataMember]
    [Display(Name = "Attribute", GroupName = "Configuration", Order = 10, Description = "Get the specified attribute of element")]
    public Argument AttributeToGet { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default };

    /// <summary>
    /// Optional input argument for <see cref="LocatorGetAttributeOptions"/> that can be used to customize the get attribute operation
    /// </summary>
    [DataMember]
    [Display(Name = "Get Attribute Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorGetAttributeOptions")]
    public Argument GetAttributeOptions { get; set; } = new InArgument<LocatorGetAttributeOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Output argument to store the attribute value of element
    /// </summary>
    [DataMember]
    [Display(Name = "Result", GroupName = "Output", Order = 10, Description = "Output argument to store the retrieved attribute value")]
    public Argument Result { get; set; } = new OutArgument<string>() { Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public GetAttributeActorComponent() : base("Get Attribute", "GetAttribute")
    {

    }

    /// <summary>
    /// Get specified attribute of element using GetAttributeAsync()
    /// </summary>
    public override async Task ActAsync()
    {
        var attributeToGet = await this.ArgumentProcessor.GetValueAsync<string>(this.AttributeToGet);
        var getAttributeOptions = this.GetAttributeOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorGetAttributeOptions>(this.GetAttributeOptions) : null;
        var (name, control) = await GetTargetControl();
        var result = await control.GetAttributeAsync(attributeToGet, getAttributeOptions);
        await this.ArgumentProcessor.SetValueAsync<string>(this.Result, result);
        logger.Information("Attribute : '{0}' having value : '{1}' was retrieved from control : '{2}'", this.AttributeToGet, result, name);
    }

}
