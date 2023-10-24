using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToContainTextActorComponent"/> to ensure that <see cref="ILocator"/> points to an element that contains the given text.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Contain Text", "Playwright", "Expect", iconSource: null, description: "Ensures the ILocator points to an element points to an element that contains the given text.", tags: new string[] { "To", "Contain", "Text", "Assert", "Expect" })]
public class ExpectToContainTextActorComponent : PlaywrightActorComponent
{
    [DataMember]
    [Display(Name = "Text", GroupName = "Configuration", Order = 10, Description = "Input argument for expected text")]
    [AllowedTypes(typeof(string), typeof(Regex), typeof(IEnumerable<string>), typeof(IEnumerable<Regex>))]
    public Argument Text { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, CanChangeType = true };

    [DataMember]
    [Display(Name = "To Contain Text Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorAssertionsToContainTextOptions")]
    public Argument ToContainTextOptions { get; set; } = new InArgument<LocatorAssertionsToContainTextOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToContainTextActorComponent() : base("Expect To Contain Text", "ExpectToContainText")
    {

    }

    /// <summary>
    /// Ensure that <see cref="ILocator"/> points to an element that contains the given text.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var (name, control) = await this.GetTargetControl();
        var options = this.ToContainTextOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorAssertionsToContainTextOptions>(this.ToContainTextOptions) : null;
        switch (this.Text)
        {
            case InArgument<string>:
                var text = await this.ArgumentProcessor.GetValueAsync<string>(this.Text);
                await Assertions.Expect(control).ToContainTextAsync(text, options);
                break;
            case InArgument<Regex>:
                var regex = await this.ArgumentProcessor.GetValueAsync<Regex>(this.Text);
                await Assertions.Expect(control).ToContainTextAsync(regex, options);
                break;
            case InArgument<IEnumerable<string>>:
                var textCollection = await this.ArgumentProcessor.GetValueAsync<IEnumerable<string>>(this.Text);
                await Assertions.Expect(control).ToContainTextAsync(textCollection, options);
                break;
            case InArgument<IEnumerable<Regex>>:
                var regExCollection = await this.ArgumentProcessor.GetValueAsync<IEnumerable<Regex>>(this.Text);
                await Assertions.Expect(control).ToContainTextAsync(regExCollection, options);
                break;
        }
    }
}

