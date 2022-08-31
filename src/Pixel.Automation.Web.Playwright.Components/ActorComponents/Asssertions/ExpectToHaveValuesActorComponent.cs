using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToHaveValuesActorComponent"/> to ensure that <see cref="ILocator"/> points to multi-select/combobox (i.e. a <c>select</c>
/// with the <c>multiple</c> attribute) and the specified values are selected.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Have Values", "Playwright", "Expect", iconSource: null, description: "Ensures the ILocator points to multi-select/combobox (i.e. a <c>select</c> with the <c>multiple</c> attribute) and the specified values are selected" , tags: new string[] { "To", "Have", "Values", "Assert", "Expect" })]
public class ExpectToHaveValuesActorComponent : PlaywrightActorComponent
{
    [DataMember]
    [Display(Name = "Values", GroupName = "Configuration", Order = 10, Description = "Input argument for expected Values")]
    [AllowedTypes(typeof(IEnumerable<string>), typeof(IEnumerable<Regex>))]
    public Argument Values { get; set; } = new InArgument<IEnumerable<string>>() { Mode = ArgumentMode.DataBound, CanChangeType = true };

    [DataMember]
    [Display(Name = "To Have Value Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorAssertionsToHaveValuesOptions")]
    public Argument ToHaveValuesOptions { get; set; } = new InArgument<LocatorAssertionsToHaveValuesOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToHaveValuesActorComponent() : base("Expect To Have Values", "ExpectToHaveValues")
    {

    }

    /// <summary>
    /// Ensure that <see cref="ILocator"/> points to multi-select/combobox (i.e. a <c>select</c> with the <c>multiple</c> attribute) and the specified values are selected.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var control = await this.GetTargetControl();       
        var options = this.ToHaveValuesOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorAssertionsToHaveValuesOptions>(this.ToHaveValuesOptions) : null;      
        switch (this.Values)
        {
            case InArgument<IEnumerable<string>>:
                var values = await this.ArgumentProcessor.GetValueAsync<IEnumerable<string>>(this.Values);
                await Assertions.Expect(control).ToHaveValuesAsync(values, options);
                break;
            case InArgument<IEnumerable<Regex>>:
                var valuesRegex = await this.ArgumentProcessor.GetValueAsync<IEnumerable<Regex>>(this.Values);
                await Assertions.Expect(control).ToHaveValuesAsync(valuesRegex, options);
                break;           
        }
    }
}

