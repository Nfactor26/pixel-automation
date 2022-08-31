using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToHaveCountActorComponent"/> to ensure that <see cref="ILocator"/> resolves to an exact number of DOM nodes.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Have Count", "Playwright", "Expect", iconSource: null, description: "Ensures the ILocator resolves to an exact number of DOM nodes.", tags: new string[] { "To", "Have", "Count", "Assert", "Expect" })]
public class ExpectToHaveCountActorComponent : PlaywrightActorComponent
{
    [DataMember]
    [Display(Name = "Count", GroupName = "Configuration", Order = 10, Description = "Input argument for expected count")]
    public Argument Count { get; set; } = new InArgument<int>() { Mode = ArgumentMode.Default };

    [DataMember]
    [Display(Name = "To Have Count Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorAssertionsToHaveCountOptions")]
    public Argument ToHaveCountOptions { get; set; } = new InArgument<LocatorAssertionsToHaveCountOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToHaveCountActorComponent() : base("Expect To Have Count", "ExpectToHaveCount")
    {

    }

    /// <summary>
    /// Ensure that <see cref="ILocator"/> resolves to an exact number of DOM nodes.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var control = await this.GetTargetControl();
        var count = await this.ArgumentProcessor.GetValueAsync<int>(this.Count);
        var options = this.ToHaveCountOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorAssertionsToHaveCountOptions>(this.ToHaveCountOptions) : null;
        await Assertions.Expect(control).ToHaveCountAsync(count, options);
    }
}

