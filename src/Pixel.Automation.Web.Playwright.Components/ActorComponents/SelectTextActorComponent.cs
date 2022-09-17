using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="SelectTextActorComponent"/> to focus the element and selects all its text content.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Select Text", "Playwright", iconSource: null, description: "Focuses the element and selects all its text content", tags: new string[] { "select text", "select", "text", "Web" })]

public class SelectTextActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<SelectTextActorComponent>();

    /// <summary>
    /// Optional input argument for <see cref="LocatorSelectTextOptions"/> that can be used to customize the select text operation
    /// </summary>
    [DataMember]
    [Display(Name = "Select Text Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for LocatorSelectTextOptions")]
    public Argument SelectTextOptions { get; set; } = new InArgument<LocatorSelectTextOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };


    /// <summary>
    /// Constructor
    /// </summary>
    public SelectTextActorComponent() : base("Select Text", "SelectText")
    {

    }

    /// <summary>
    /// Select text using SelectTextAsync() method.
    /// </summary>
    public override async Task ActAsync()
    {            
        var control = await GetTargetControl();
        var options = this.SelectTextOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorSelectTextOptions>(this.SelectTextOptions) : null;
        await control.SelectTextAsync(options);
        logger.Information("Select text operation performed on element.");        
    }

}
