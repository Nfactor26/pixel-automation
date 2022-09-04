using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="GoBackActorComponent"/> to navigate the previous page in history.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Go Back", "Playwright", "Browser", iconSource: null, description: "Navigate to the previous page in history", tags: new string[] { "GoBack", "Back", "Navigate", "Browser", "Web" })]
public class GoBackActorComponent : PlaywrightActorComponent
{    
    /// <summary>
    /// Optional input argument for <see cref="PageGoBackOptions"/> that can be used to customize the reload operation
    /// </summary>
    [DataMember]
    [Display(Name = "GoBack Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for PageGoBackOptions")]
    public Argument GoBackOptions { get; set; } = new InArgument<PageGoBackOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    [DataMember]
    [Display(Name ="Result", GroupName = "Output", Order = 10, Description = "[Optional] Output argument to store response result")]
    public Argument Result { get; set; } = new OutArgument<IResponse?>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public GoBackActorComponent():base("Go Back", "GoBack")
    {

    }

    /// <summary>
    /// Navigate to the previous page in history
    /// </summary>
    public override async Task ActAsync()
    {
        var goBackOptions = this.GoBackOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<PageGoBackOptions>(this.GoBackOptions) : null;
        var result = await this.ApplicationDetails.ActivePage.GoBackAsync(goBackOptions);
        if(this.Result.IsConfigured())
        {
            await this.ArgumentProcessor.SetValueAsync<IResponse?>(Result, result);
        }       
    }
}
