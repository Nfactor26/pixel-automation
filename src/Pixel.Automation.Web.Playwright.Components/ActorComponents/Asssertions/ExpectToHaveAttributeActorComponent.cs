using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToHaveAttributeActorComponent"/> to ensure that <see cref="ILocator"/> points to an element with given attribute.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Have Attribute", "Playwright", "Expect", iconSource: null, description: "Ensures the ILocator points to an element with given attribute.", tags: new string[] { "To", "Have", "Attribute", "Assert", "Expect" })]
public class ExpectToHaveAttributeActorComponent : PlaywrightActorComponent
{
    [DataMember(IsRequired = true)]
    [Display(Name = "Attribute Name", GroupName = "Configuration", Order = 10, Description = "Input argument for attribute name")]   
    public Argument AttributeName { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default };

    [DataMember(IsRequired = true)]
    [Display(Name = "Attribute Value", GroupName = "Configuration", Order = 20, Description = "Input argument for expected attribute value")]
    [AllowedTypes(typeof(string), typeof(Regex))]
    public Argument AttributeValue { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, CanChangeType = true };

    [DataMember]
    [Display(Name = "To Have Attribute Options", GroupName = "Configuration", Order = 30, Description = "[Optional] Input argument for LocatorAssertionsToHaveAttributeOptions")]
    public Argument ToHaveAttributeOptions { get; set; } = new InArgument<LocatorAssertionsToHaveAttributeOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToHaveAttributeActorComponent() : base("Expect To Have Attribute", "ExpectToHaveAttribute")
    {

    }

    /// <summary>
    /// Ensure that <see cref="ILocator"/>points to an element with given attribute.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var (name, control) = await this.GetTargetControl();
        var attributeName = await this.ArgumentProcessor.GetValueAsync<string>(this.AttributeName);
        var toHaveAttributeOptions = this.ToHaveAttributeOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorAssertionsToHaveAttributeOptions>(this.ToHaveAttributeOptions) : null;
        switch (this.AttributeValue)
        {
            case InArgument<string>:
                var attributeValue = await this.ArgumentProcessor.GetValueAsync<string>(this.AttributeValue);
                await Assertions.Expect(control).ToHaveAttributeAsync(attributeName, attributeValue, toHaveAttributeOptions);
                break;
            case InArgument<Regex>:
                var attributeValueRegex = await this.ArgumentProcessor.GetValueAsync<Regex>(this.AttributeValue);
                await Assertions.Expect(control).ToHaveAttributeAsync(attributeName, attributeValueRegex, toHaveAttributeOptions);
                break;
        }
    }
}
