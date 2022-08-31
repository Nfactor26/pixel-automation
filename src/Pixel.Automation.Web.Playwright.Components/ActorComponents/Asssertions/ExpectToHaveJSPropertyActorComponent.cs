using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToHaveJSPropertyActorComponent"/> to ensures that <see cref="ILocator"/> points to an element with given JavaScript property.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Have JS Property", "Playwright", "Expect", iconSource: null, description: "Ensures that ILocator points to an element with given JavaScript property", tags: new string[] { "To", "Have", "JS Property" , "Assert", "Expect" })]
public class ExpectToHaveJSPropertyActorComponent : PlaywrightActorComponent
{
    [DataMember(IsRequired = true)]
    [Display(Name = "Name", GroupName = "Configuration", Order = 10, Description = "Input argument for name of js property")]
    public Argument PropertyName { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default };

    [DataMember(IsRequired = true)]
    [Display(Name = "Value", GroupName = "Configuration", Order = 20, Description = "Input argument for value of js property")]
    public Argument PropertyValue { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default };

    [DataMember]
    [Display(Name = "Js Property Options", GroupName = "Configuration", Order = 30, Description = "[Optional] Input argument for LocatorAssertionsToHaveJSPropertyOptions")]
    public Argument ToHaveJsPropertyOptions { get; set; } = new InArgument<LocatorAssertionsToHaveJSPropertyOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToHaveJSPropertyActorComponent() : base("Expect To Have JS Property", "ExpectToHaveJSProperty")
    {

    }

    /// <summary>
    /// Ensures that <see cref="ILocator"/> points to an element with given JavaScript property
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var control = await GetTargetControl();
        var name = await this.ArgumentProcessor.GetValueAsync<string>(this.PropertyName);
        var value = await this.ArgumentProcessor.GetValueAsync<string>(this.PropertyValue);
        var toHaveJsPropertyOptions = this.ToHaveJsPropertyOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorAssertionsToHaveJSPropertyOptions>(this.ToHaveJsPropertyOptions) : null;
        await Assertions.Expect(control).ToHaveJSPropertyAsync(name, value, toHaveJsPropertyOptions);
    }
}

