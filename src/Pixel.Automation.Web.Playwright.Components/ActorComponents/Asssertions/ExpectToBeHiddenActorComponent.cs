using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToBeHiddenActorComponent"/> to ensure that <see cref="ILocator"/> points to a hidden DOM node.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Be Hidden", "Playwright", "Expect", iconSource: null, description: "Ensures the ILocator points to a hidden DOM node", tags: new string[] { "To", "Be", "Hidden", "Assert", "Expect" })]
public class ExpectToBeHiddenActorComponent : PlaywrightActorComponent
{   
    [DataMember]
    [Display(Name = "To Be Hidden Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for LocatorAssertionsToBeHiddenOptions")]
    public Argument ToBeHiddenOptions { get; set; } = new InArgument<LocatorAssertionsToBeHiddenOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToBeHiddenActorComponent() : base("Expect To Be Hidden", "ExpectToBeHidden")
    {

    }

    /// <summary>
    /// Ensure that <see cref="ILocator"/> points to a hidden DOM node.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var control = await this.GetTargetControl();  
        var options = this.ToBeHiddenOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorAssertionsToBeHiddenOptions>(this.ToBeHiddenOptions) : null;
        await Assertions.Expect(control).ToBeHiddenAsync(options);
    }
}

