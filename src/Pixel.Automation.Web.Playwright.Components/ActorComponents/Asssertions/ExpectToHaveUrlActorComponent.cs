using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="ExpectToHaveUrlActorComponent"/> to ensure the page is navigated to the given URL..
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("To Have Url", "Playwright", "Expect", iconSource: null, description: "Check a checkbox or a radio button", tags: new string[] { "To", "Have", "Url", "Assert", "Expect" })]
public class ExpectToHaveUrlActorComponent : PlaywrightActorComponent
{
    [DataMember(IsRequired = true)]
    [Display(Name = "Url", GroupName = "Configuration", Order = 10, Description = "Input argument for url to look for")]
    [AllowedTypes(typeof(string), typeof(Regex))]
    public Argument Url { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default , CanChangeType = true };
  
    [DataMember]
    [Display(Name = "To Have Url Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for PageAssertionsToHaveURLOptions")]
    public Argument ToHaveUrlOptions { get; set; } = new InArgument<PageAssertionsToHaveURLOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectToHaveUrlActorComponent() : base("Expect To Have Url", "ExpectToHaveUrl")
    {

    }

    /// <summary>
    /// Ensures the page is navigated to the given URL.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {       
        var toHaveUrlOptions = this.ToHaveUrlOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<PageAssertionsToHaveURLOptions>(this.ToHaveUrlOptions) : null;
        
        switch (this.Url)
        {
            case InArgument<string>:
                var title = await this.ArgumentProcessor.GetValueAsync<string>(this.Url);
                await Assertions.Expect(this.ApplicationDetails.ActivePage).ToHaveURLAsync(title, toHaveUrlOptions);
                break;
            case InArgument<Regex>:
                var regex = await this.ArgumentProcessor.GetValueAsync<Regex>(this.Url);
                await Assertions.Expect(this.ApplicationDetails.ActivePage).ToHaveURLAsync(regex, toHaveUrlOptions);
                break;
        }
    }
}
