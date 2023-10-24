using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToBeEnabledActorComponent"/> to ensure that <see cref="ILocator"/> point to an empty editable element or to a DOM.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Be Empty", "Playwright", "Expect", iconSource: null, description: "Ensures the ILocator points to an empty editable element or to a DOM", tags: new string[] { "To", "Be", "Empty", "Assert", "Expect" })]
public class ExpectToBeEmptyActorComponent : PlaywrightActorComponent
{   
    [DataMember]
    [Display(Name = "To Be Empty Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for LocatorAssertionsToBeEmptyOptions")]
    public Argument ToBeEmptyOptions { get; set; } = new InArgument<LocatorAssertionsToBeEmptyOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToBeEmptyActorComponent() : base("Expect To Be Empty", "ExpectToBeEmpty")
    {

    }

    /// <summary>
    /// Ensure that <see cref="ILocator"/> point to an empty editable element or to a DOM.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var (name, control) = await this.GetTargetControl();  
        var options = this.ToBeEmptyOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorAssertionsToBeEmptyOptions>(this.ToBeEmptyOptions) : null;
        await Assertions.Expect(control).ToBeEmptyAsync(options);
    }
}

