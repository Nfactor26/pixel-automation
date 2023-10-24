using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToBeCheckedActorComponent"/> to ensure that <see cref="ILocator"/> points to a checked input.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Be Checked", "Playwright", "Expect", iconSource: null, description: "Ensures the Locator points to a checked input.", tags: new string[] { "To", "Be", "Checked", "Assert", "Expect" })]
public class ExpectToBeCheckedActorComponent : PlaywrightActorComponent
{   
    [DataMember]
    [Display(Name = "To Be Checked Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for LocatorAssertionsToBeCheckedOptions")]
    public Argument ToBeCheckedOptions { get; set; } = new InArgument<LocatorAssertionsToBeCheckedOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToBeCheckedActorComponent() : base("Expect To Be Checked", "ExpectToBeChecked")
    {

    }

    /// <summary>
    /// Ensure that <see cref="ILocator"/> points to a checked input.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var (name, control) = await this.GetTargetControl();  
        var options = this.ToBeCheckedOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorAssertionsToBeCheckedOptions>(this.ToBeCheckedOptions) : null;
        await Assertions.Expect(control).ToBeCheckedAsync(options);
    }
}

