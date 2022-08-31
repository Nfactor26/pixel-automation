using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToBeEnabledActorComponent"/> to ensure that <see cref="ILocator"/> point to a enabled element.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Be Enabled", "Playwright", "Expect", iconSource: null, description: "Ensures the ILocator points to a enabled element.", tags: new string[] { "To", "Be", "Enabled", "Assert", "Expect" })]
public class ExpectToBeEnabledActorComponent : PlaywrightActorComponent
{   
    [DataMember]
    [Display(Name = "To Be Enabled Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for LocatorAssertionsToBeEnabledOptions")]
    public Argument ToBeEnabledOptions { get; set; } = new InArgument<LocatorAssertionsToBeEnabledOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToBeEnabledActorComponent() : base("Expect To Be Enabled", "ExpectToBeEnabled")
    {

    }

    /// <summary>
    /// Ensure that <see cref="ILocator"/> point to a enabled element.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var control = await this.GetTargetControl();  
        var options = this.ToBeEnabledOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorAssertionsToBeEnabledOptions>(this.ToBeEnabledOptions) : null;
        await Assertions.Expect(control).ToBeEnabledAsync(options);
    }
}

