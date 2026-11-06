using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components.ActorComponents;

[DataContract]
[Serializable]
[ToolBoxItem("Wait For Url", "Playwright", iconSource: null, description: "Wait for url", tags: new string[] { "Wait", "Url", "Wait for Url" })]
public class WaitForUrlActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<WaitForUrlActorComponent>();

    [DataMember]
    [Display(Name = "Url To Wait For", GroupName = "Configuration", Order = 10, Description = "URL to wait for")]
    public Argument Url { get; set; } = new InArgument<string>() { CanChangeType = false, Mode = ArgumentMode.Default };

    /// <summary>
    /// Optional input argument for <see cref="PageWaitForURLOptions"/> that can be used to customize the wait for url operation
    /// </summary>
    [DataMember]
    [Display(Name = "Wait For Url Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for PageWaitForURLOptions")]
    public Argument WaitForUrlOptions { get; set; } = new InArgument<PageWaitForURLOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    ///C onstructor
    /// </summary>
    public WaitForUrlActorComponent() : base("Run And Wait For Navigation", "RunAndWaitForNavigation")
    {

    }

    public override async Task ActAsync()
    {
        var url = await this.ArgumentProcessor.GetValueAsync<string>(this.Url);
        var waitForOptions = this.WaitForUrlOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<PageWaitForURLOptions>(this.WaitForUrlOptions) : null;
        var activePage = this.ApplicationDetails.ActivePage;
        await activePage.WaitForURLAsync(url, waitForOptions);
    }
}
