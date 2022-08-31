using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToHaveValueActorComponent"/> to ensure that <see cref="ILocator"/> points to an element with the given input value.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Have Value", "Playwright", "Expect", iconSource: null, description: "Ensures the ILocator points to an element with the given input value.", tags: new string[] { "To", "Have", "Value", "Assert", "Expect" })]
public class ExpectToHaveValueActorComponent : PlaywrightActorComponent
{
    [DataMember]
    [Display(Name = "Value", GroupName = "Configuration", Order = 10, Description = "Input argument for expected Value")]
    [AllowedTypes(typeof(string), typeof(Regex))]
    public Argument Value { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, CanChangeType = true };

    [DataMember]
    [Display(Name = "To Have Value Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorAssertionsToHaveValueOptions")]
    public Argument ToHaveValueOptions { get; set; } = new InArgument<LocatorAssertionsToHaveValueOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToHaveValueActorComponent() : base("Expect To Have Value", "ExpectToHaveValue")
    {

    }

    /// <summary>
    /// Ensure that <see cref="ILocator"/> points to an element with the given input value..
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var control = await this.GetTargetControl();       
        var options = this.ToHaveValueOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorAssertionsToHaveValueOptions>(this.ToHaveValueOptions) : null;      
        switch (this.Value)
        {
            case InArgument<string>:
                var value = await this.ArgumentProcessor.GetValueAsync<string>(this.Value);
                await Assertions.Expect(control).ToHaveValueAsync(value, options);
                break;
            case InArgument<Regex>:
                var regex = await this.ArgumentProcessor.GetValueAsync<Regex>(this.Value);
                await Assertions.Expect(control).ToHaveValueAsync(regex, options);
                break;           
        }
    }
}

