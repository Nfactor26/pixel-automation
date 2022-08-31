using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToHaveIdActorComponent"/> to ensure that <see cref="ILocator"/> points to an element with the given DOM Node.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Have Id", "Playwright", "Expect", iconSource: null, description: "Ensures the ILocator points to an element with the given DOM Node.", tags: new string[] { "To", "Have", "Id", "Assert", "Expect" })]
public class ExpectToHaveIdActorComponent : PlaywrightActorComponent
{
    [DataMember(IsRequired = true)]
    [Display(Name = "Id", GroupName = "Configuration", Order = 10, Description = "Input argument for Id")]
    [AllowedTypes(typeof(string), typeof(Regex))]
    public Argument Identifier { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default , CanChangeType = true };

    [DataMember]
    [Display(Name = "To Have Id Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for PageAssertionsToHaveURLOptions")]
    public Argument ToHaveIdOptions { get; set; } = new InArgument<LocatorAssertionsToHaveIdOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToHaveIdActorComponent() : base("Expect To Have Id", "ExpectToHaveId")
    {

    }

    /// <summary>
    /// Ensure that <see cref="ILocator"/> points to an element with the given DOM Node.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var control = await this.GetTargetControl();       
        var toHaveIdOptions = this.ToHaveIdOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorAssertionsToHaveIdOptions>(this.ToHaveIdOptions) : null;
        switch (this.Identifier)
        {
            case InArgument<string>:
                var identifier = await this.ArgumentProcessor.GetValueAsync<string>(this.Identifier);
                await Assertions.Expect(control).ToHaveIdAsync(identifier, toHaveIdOptions);
                break;
            case InArgument<Regex>:
                var regex = await this.ArgumentProcessor.GetValueAsync<Regex>(this.Identifier);
                await Assertions.Expect(control).ToHaveIdAsync(regex, toHaveIdOptions);
                break;
        }
    }
}
