using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components.Processors;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

[DataContract]
[Serializable]
[ToolBoxItem("Run And Wait For Download", "Playwright", "Run And Wait", iconSource: null, description: "Run and wait for Download", tags: new string[] { "Run", "Wait", "Download", "Web" })]
public class RunAndWaitForDownloadChooserProcessor : EntityProcessor
{
    /// <summary>
    /// Owner application that is interacted with
    /// </summary>      
    [Browsable(false)]
    public WebApplication ApplicationDetails
    {
        get
        {
            return this.EntityManager.GetOwnerApplication<WebApplication>(this);
        }
    }

    /// <summary>
    /// Optional input argument for <see cref="PageRunAndWaitForDownloadOptions"/> that can be used to customize the run and wait for Download operation
    /// </summary>
    [DataMember]
    [Display(Name = "Wait For Download Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for PageRunAndWaitForDownloadOptions")]
    public Argument WaitForDownloadOptions { get; set; } = new InArgument<PageRunAndWaitForDownloadOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public RunAndWaitForDownloadChooserProcessor() : base(" Run And Wait For Download", " RunAndWaitForDownload")
    {

    }

    public override async Task BeginProcessAsync()
    {
        var waitForDownloadOptions = this.WaitForDownloadOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<PageRunAndWaitForDownloadOptions>(this.WaitForDownloadOptions) : null;
        var activePage = this.ApplicationDetails.ActivePage;
        await activePage.RunAndWaitForDownloadAsync(async () =>
        {
            await ProcessEntity(this);
        }, waitForDownloadOptions);
    }
}
