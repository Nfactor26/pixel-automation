using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToHaveTitleActorComponent"/> to ensure the page has the given title.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Have Title", "Playwright", "Expect", iconSource: null, description: "Ensures the page has the given title.", tags: new string[] { "To", "Have", "Title", "Assert", "Expect" })]
public class ExpectToHaveTitleActorComponent : PlaywrightActorComponent
{
    [DataMember(IsRequired = true)]
    [Display(Name = "Title", GroupName = "Configuration", Order = 10, Description = "Input argument for title to look for")]
    [AllowedTypes(typeof(string), typeof(Regex))]
    public Argument Title { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, CanChangeType = true };  

    [DataMember]
    [Display(Name = "Title Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for PageAssertionsToHaveTitleOptions")]
    public Argument ToHaveTitleOptions { get; set; } = new InArgument<PageAssertionsToHaveTitleOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToHaveTitleActorComponent() : base("Expect To Have Title", "ExpectToHaveTitle")
    {

    }

    /// <summary>
    /// Ensure the page has the given title.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {        
        var toHaveTitleOptions = this.ToHaveTitleOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<PageAssertionsToHaveTitleOptions>(this.ToHaveTitleOptions) : null;

        switch (this.Title)
        {
            case InArgument<string>:
                var title =  await this.ArgumentProcessor.GetValueAsync<string>(this.Title);
                await Assertions.Expect(this.ApplicationDetails.ActivePage).ToHaveTitleAsync(title, toHaveTitleOptions);
                break;
            case InArgument<Regex>:
                var regex = await this.ArgumentProcessor.GetValueAsync<Regex>(this.Title);
                await Assertions.Expect(this.ApplicationDetails.ActivePage).ToHaveTitleAsync(regex, toHaveTitleOptions);
                break;
        }
    }
}
