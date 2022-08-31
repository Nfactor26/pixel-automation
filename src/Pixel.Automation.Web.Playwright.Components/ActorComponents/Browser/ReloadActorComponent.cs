using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="GotoActorComponent"/> to navigate the browser to a specified url.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Reload", "Playwright", "Browser", iconSource: null, description: "Navigate the browser to desired url", tags: new string[] { "Reload", "Navigate", "Browser", "Web" })]
public class ReloadActorComponent : PlaywrightActorComponent
{    
    /// <summary>
    /// Optional input argument for <see cref="PageReloadOptions"/> that can be used to customize the reload operation
    /// </summary>
    [DataMember]
    [Display(Name = "Reload Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for PageReloadOptions")]
    public Argument ReloadOptions { get; set; } = new InArgument<PageReloadOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    [DataMember]
    [Display(Name ="Result", GroupName = "Output", Order = 10, Description = "[Optional] Output argument to store response result")]
    public Argument Result { get; set; } = new OutArgument<IResponse?>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public ReloadActorComponent():base("Reload", "Reload")
    {

    }

    /// <summary>
    /// Navigate the active window/tab of the brower to a specified url using GotoAsync() method.
    /// </summary>
    public override async Task ActAsync()
    {
        var reloadOptions = this.ReloadOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<PageReloadOptions>(this.ReloadOptions) : null;
        var result = await this.ApplicationDetails.ActivePage.ReloadAsync(reloadOptions);
        if(this.Result.IsConfigured())
        {
            await this.ArgumentProcessor.SetValueAsync<IResponse?>(Result, result);
        }       
    }
}
