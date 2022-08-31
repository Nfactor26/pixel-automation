using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToBeEditableActorComponent"/> ensure <see cref="ILocator"/> points to an editable element.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Be Editable", "Playwright", "Expect", iconSource: null, description: "Ensures the ILocator points to an editable element.", tags: new string[] { "To", "Be", "Editable", "Assert", "Expect" })]
public class ExpectToBeEditableActorComponent : PlaywrightActorComponent
{   
    [DataMember]
    [Display(Name = "To Be Editable Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for LocatorAssertionsToBeEditableOptions")]
    public Argument ToBeEditableOptions { get; set; } = new InArgument<LocatorAssertionsToBeEditableOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToBeEditableActorComponent() : base("Expect To Be Editable", "ExpectToBeEditable")
    {

    }

    /// <summary>
    /// Ensure <see cref="ILocator"/> points to an editable element.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var control = await this.GetTargetControl();  
        var options = this.ToBeEditableOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorAssertionsToBeEditableOptions>(this.ToBeEditableOptions) : null;
        await Assertions.Expect(control).ToBeEditableAsync(options);
    }
}

