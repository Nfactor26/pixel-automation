using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="GoForwardActorComponent"/> to navigate to the next page in history.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Go Forward", "Playwright", "Browser", iconSource: null, description: "Navigate to the next page in history", tags: new string[] { "GoForward", "Forward", "Navigate", "Browser", "Web" })]
public class GoForwardActorComponent : PlaywrightActorComponent
{    
    /// <summary>
    /// Optional input argument for <see cref="PageGoForwardOptions"/> that can be used to customize the reload operation
    /// </summary>
    [DataMember]
    [Display(Name = "GoForward Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for PageGoForwardOptions")]
    public Argument GoForwardOptions { get; set; } = new InArgument<PageGoForwardOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    [DataMember]
    [Display(Name ="Result", GroupName = "Output", Order = 10, Description = "[Optional] Output argument to store response result")]
    public Argument Result { get; set; } = new OutArgument<IResponse?>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public GoForwardActorComponent():base("GoForward", "GoForward")
    {

    }

    /// <summary>
    /// Navigate to the next page in history
    /// </summary>
    public override async Task ActAsync()
    {
        var goForwardOptions = this.GoForwardOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<PageGoForwardOptions>(this.GoForwardOptions) : null;
        var result = await this.ApplicationDetails.ActivePage.GoForwardAsync(goForwardOptions);
        if(this.Result.IsConfigured())
        {
            await this.ArgumentProcessor.SetValueAsync<IResponse?>(Result, result);
        }       
    }
}
