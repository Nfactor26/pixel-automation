using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToBeFocusedActorComponent"/> to ensure that <see cref="ILocator"/> points to a focused DOM node.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Be Focused", "Playwright", "Expect", iconSource: null, description: "Ensures the ILocator points to a focused DOM node.", tags: new string[] { "To", "Be", "Focused", "Assert", "Expect" })]
public class ExpectToBeFocusedActorComponent : PlaywrightActorComponent
{   
    [DataMember]
    [Display(Name = "To Be Focused Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for LocatorAssertionsToBeFocusedOptions")]
    public Argument ToBeFocusedOptions { get; set; } = new InArgument<LocatorAssertionsToBeFocusedOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToBeFocusedActorComponent() : base("Expect To Be Focused", "ExpectToBeFocused")
    {

    }

    /// <summary>
    /// Ensure that <see cref="ILocator"/> points to a focused DOM node.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var control = await this.GetTargetControl();  
        var options = this.ToBeFocusedOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorAssertionsToBeFocusedOptions>(this.ToBeFocusedOptions) : null;
        await Assertions.Expect(control).ToBeFocusedAsync(options);
    }
}

