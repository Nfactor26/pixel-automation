using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="GotoActorComponent"/> to navigate the browser to a specified url.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Goto", "Playwright", "Browser", iconSource: null, description: "Navigate the browser to desired url", tags: new string[] { "goto", "navigate", "Web" })]
public class GotoActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<GotoActorComponent>();

    /// <summary>
    /// Input argument to specify Url to navigate to
    /// </summary>
    [DataMember(IsRequired = true)]   
    [Display(Name = "Url", GroupName = "Configuration", Order = 10, Description = "Input argument for url to navigate to")]        
    public Argument TargetUrl { get; set; } = new InArgument<string>() { DefaultValue ="https://www.bing.com" };


    /// <summary>
    /// Optional input argument for <see cref="PageGotoOptions"/> that can be used to customize the goto operation
    /// </summary>
    [DataMember]
    [Display(Name = "Goto Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for PageGotoOptions")]
    public Argument GotoOptions { get; set; } = new InArgument<PageGotoOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    [DataMember]
    [Display(Name ="Result", GroupName = "Output", Order = 10, Description = "[Optional] Output argument to store response result")]
    public Argument Result { get; set; } = new OutArgument<IResponse?>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public GotoActorComponent():base("Goto", "Goto")
    {

    }

    /// <summary>
    /// Navigate the active window/tab of the brower to a specified url using GotoAsync() method.
    /// </summary>
    public override async Task ActAsync()
    {
        var targetUrl = await ArgumentProcessor.GetValueAsync<string>(this.TargetUrl);
        var result = await this.ApplicationDetails.ActivePage.GotoAsync(targetUrl, this.GotoOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<PageGotoOptions>(this.GotoOptions) : null);
        if(this.Result.IsConfigured())
        {
            await this.ArgumentProcessor.SetValueAsync<IResponse?>(Result, result);
        }
        logger.Information($"Goto to Url : {targetUrl}");       
    }

    ///</inheritdoc>
    public override string ToString()
    {
        return "Goto Actor";
    }
}
