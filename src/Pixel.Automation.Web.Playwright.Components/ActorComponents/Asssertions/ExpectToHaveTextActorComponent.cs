using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToHaveTextActorComponent"/> to ensure that <see cref="ILocator"/> points to an element with the given text.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Have Text", "Playwright", "Expect", iconSource: null, description: "Ensures the ILocator points to an element with the given text.", tags: new string[] { "To", "Have", "Text", "Assert", "Expect" })]
public class ExpectToHaveTextActorComponent : PlaywrightActorComponent
{
    [DataMember]
    [Display(Name = "Text", GroupName = "Configuration", Order = 10, Description = "Input argument for expected Text")]
    [AllowedTypes(typeof(string), typeof(Regex), typeof(IEnumerable<string>), typeof(IEnumerable<Regex>))]
    public Argument Text { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, CanChangeType = true };

    [DataMember]
    [Display(Name = "To Have Text Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorAssertionsToHaveTextOptions")]
    public Argument ToHaveTextOptions { get; set; } = new InArgument<LocatorAssertionsToHaveTextOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToHaveTextActorComponent() : base("Expect To Have Text", "ExpectToHaveText")
    {

    }

    /// <summary>
    /// Ensure that <see cref="ILocator"/> points to an element with the given text.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var (name, control) = await this.GetTargetControl();       
        var options = this.ToHaveTextOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorAssertionsToHaveTextOptions>(this.ToHaveTextOptions) : null;      
        switch (this.Text)
        {
            case InArgument<string>:
                var text = await this.ArgumentProcessor.GetValueAsync<string>(this.Text);
                await Assertions.Expect(control).ToHaveTextAsync(text, options);
                break;
            case InArgument<Regex>:
                var regex = await this.ArgumentProcessor.GetValueAsync<Regex>(this.Text);
                await Assertions.Expect(control).ToHaveTextAsync(regex, options);
                break;
            case InArgument<IEnumerable<string>>:
                var textCollection = await this.ArgumentProcessor.GetValueAsync<IEnumerable<string>>(this.Text);
                await Assertions.Expect(control).ToHaveTextAsync(textCollection, options);
                break;
            case InArgument<IEnumerable<Regex>>:
                var regExCollection = await this.ArgumentProcessor.GetValueAsync<IEnumerable<Regex>>(this.Text);
                await Assertions.Expect(control).ToHaveTextAsync(regExCollection, options);
                break;
        }
    }
}

