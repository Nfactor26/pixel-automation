using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToBeDisabledActorComponent"/> to ensure that <see cref="ILocator"/> points to a disabled element.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Be Disabled", "Playwright", "Expect", iconSource: null, description: "Ensures the ILocator points to a disabled element.", tags: new string[] { "To", "Be", "Disabled", "Assert", "Expect" })]
public class ExpectToBeDisabledActorComponent : PlaywrightActorComponent
{   
    [DataMember]
    [Display(Name = "To Be Disabled Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for LocatorAssertionsToBeDisabledOptions")]
    public Argument ToBeDisabledOptions { get; set; } = new InArgument<LocatorAssertionsToBeDisabledOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToBeDisabledActorComponent() : base("Expect To Be Disabled", "ExpectToBeDisabled")
    {

    }

    /// <summary>
    /// Ensure that <see cref="ILocator"/> points to a disabled element.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var control = await this.GetTargetControl();  
        var options = this.ToBeDisabledOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorAssertionsToBeDisabledOptions>(this.ToBeDisabledOptions) : null;
        await Assertions.Expect(control).ToBeDisabledAsync(options);
    }
}

