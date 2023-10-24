using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToHaveCSSPropertyActorComponent"/> to ensure the <see cref="ILocator"/> resolves to an element with the given computed style.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Have CSS", "Playwright", "Expect", iconSource: null, description: "Ensures the Locator resolves to an element with the given computed CSS style.", tags: new string[] { "To", "Have", "CSS", "Assert", "Expect" })]
public class ExpectToHaveCSSPropertyActorComponent : PlaywrightActorComponent
{
    [DataMember(IsRequired = true)]
    [Display(Name = "Name", GroupName = "Configuration", Order = 10, Description = "Input argument for name of css property")]
    public Argument PropertyName { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default };

    [DataMember(IsRequired = true)]
    [Display(Name = "Value", GroupName = "Configuration", Order = 20, Description = "Input argument for expected value of css property")]
    [AllowedTypes(typeof(string), typeof(Regex))]
    public Argument PropertyValue { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, CanChangeType = true };

    [DataMember]
    [Display(Name = "CSS Property Options", GroupName = "Configuration", Order = 30, Description = "[Optional] Input argument for LocatorAssertionsToHaveJSPropertyOptions")]
    public Argument ToHaveCSSPropertyOptions { get; set; } = new InArgument<LocatorAssertionsToHaveCSSOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToHaveCSSPropertyActorComponent() : base("Expect To Have CSS Property", "ExpectToHaveCSSProperty")
    {

    }

    /// <summary>
    /// Ensure the <see cref="ILocator"/> resolves to an element with the given computed style.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var (controlName, control) = await GetTargetControl();
        var name = await this.ArgumentProcessor.GetValueAsync<string>(this.PropertyName);    
        var toHaveCSSPropertyOptions = this.ToHaveCSSPropertyOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorAssertionsToHaveCSSOptions>(this.ToHaveCSSPropertyOptions) : null;
        
        switch (this.PropertyValue)
        {
            case InArgument<string>:
                var value = await this.ArgumentProcessor.GetValueAsync<string>(this.PropertyValue);
                await Assertions.Expect(control).ToHaveCSSAsync(name, value, toHaveCSSPropertyOptions);
                break;
            case InArgument<Regex>:
                var regex = await this.ArgumentProcessor.GetValueAsync<Regex>(this.PropertyValue);
                await Assertions.Expect(control).ToHaveCSSAsync(name, regex, toHaveCSSPropertyOptions);
                break;
        }
    }
}

