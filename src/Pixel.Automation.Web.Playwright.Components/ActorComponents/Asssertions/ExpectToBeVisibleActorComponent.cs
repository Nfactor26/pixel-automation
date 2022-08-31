using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToBeVisibleActorComponent"/> to ensure that <see cref="ILocator"/> points to a Visible DOM node.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Be Visible", "Playwright", "Expect", iconSource: null, description: "Ensures the ILocator points to a Visible DOM node", tags: new string[] { "To", "Be", "Visible", "Assert", "Expect" })]
public class ExpectToBeVisibleActorComponent : PlaywrightActorComponent
{   
    [DataMember]
    [Display(Name = "To Be Visible Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for LocatorAssertionsToBeVisibleOptions")]
    public Argument ToBeVisibleOptions { get; set; } = new InArgument<LocatorAssertionsToBeVisibleOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToBeVisibleActorComponent() : base("Expect To Be Visible", "ExpectToBeVisible")
    {

    }

    /// <summary>
    /// Ensure that <see cref="ILocator"/> points to a Visible DOM node.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var control = await this.GetTargetControl();  
        var options = this.ToBeVisibleOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorAssertionsToBeVisibleOptions>(this.ToBeVisibleOptions) : null;
        await Assertions.Expect(control).ToBeVisibleAsync(options);
    }
}

