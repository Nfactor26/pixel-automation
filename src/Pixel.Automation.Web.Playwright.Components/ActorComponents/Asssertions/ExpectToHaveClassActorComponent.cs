using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToHaveClassActorComponent"/> to ensure that <see cref="ILocator"/> points to an element with given CSS class.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Have Class", "Playwright", "Expect", iconSource: null, description: "Ensures the ILocator points to an element with given CSS class.", tags: new string[] { "To", "Have", "Class", "Assert", "Expect" })]
public class ExpectToHaveClassActorComponent : PlaywrightActorComponent
{
    [DataMember]
    [Display(Name = "Class", GroupName = "Configuration", Order = 10, Description = "Input argument for expected class")]
    [AllowedTypes(typeof(string), typeof(Regex), typeof(IEnumerable<string>), typeof(IEnumerable<Regex>))]
    public Argument Class { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, CanChangeType = true };

    [DataMember]
    [Display(Name = "To Have Class Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorAssertionsToHaveClassOptions")]
    public Argument ToHaveClassOptions { get; set; } = new InArgument<LocatorAssertionsToHaveClassOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToHaveClassActorComponent() : base("Expect To Have Class", "ExpectToHaveClass")
    {

    }

    /// <summary>
    /// Ensure that <see cref="ILocator"/> points to an element with given CSS class.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var control = await this.GetTargetControl();       
        var options = this.ToHaveClassOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorAssertionsToHaveClassOptions>(this.ToHaveClassOptions) : null;      
        switch (this.Class)
        {
            case InArgument<string>:
                var Class = await this.ArgumentProcessor.GetValueAsync<string>(this.Class);
                await Assertions.Expect(control).ToHaveClassAsync(Class, options);
                break;
            case InArgument<Regex>:
                var regex = await this.ArgumentProcessor.GetValueAsync<Regex>(this.Class);
                await Assertions.Expect(control).ToHaveClassAsync(regex, options);
                break;
            case InArgument<IEnumerable<string>>:
                var ClassCollection = await this.ArgumentProcessor.GetValueAsync<IEnumerable<string>>(this.Class);
                await Assertions.Expect(control).ToHaveClassAsync(ClassCollection, options);
                break;
            case InArgument<IEnumerable<Regex>>:
                var regExCollection = await this.ArgumentProcessor.GetValueAsync<IEnumerable<Regex>>(this.Class);
                await Assertions.Expect(control).ToHaveClassAsync(regExCollection, options);
                break;
        }
    }
}

