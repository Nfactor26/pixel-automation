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
[ToolBoxItem("Run And Wait For Request Finished", "Playwright", "Run And Wait", iconSource: null, description: "Run and wait for request", tags: new string[] { "Run", "Wait", "Request", "Finished", "Web" })]
public class RunAndWaitForRequestFinishedProcessor : EntityProcessor
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
    /// Optional input argument for <see cref="PageRunAndWaitForRequestFinishedOptions"/> that can be used to customize the run and wait for request finished operation
    /// </summary>
    [DataMember]
    [Display(Name = "Wait For Request Options", GroupName = "Configuration", Order = 20, Description = "Input argument for PageRunAndWaitForRequestOptions")]
    public Argument WaitForRequestOptions { get; set; } = new InArgument<PageRunAndWaitForRequestFinishedOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public RunAndWaitForRequestFinishedProcessor() : base("Run And Wait For Request Finished", "RunAndWaitForRequestFinished")
    {

    }

    public override async Task BeginProcessAsync()
    {
        var waitForRequestFinishedOptions = this.WaitForRequestOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<PageRunAndWaitForRequestFinishedOptions>(this.WaitForRequestOptions) : null;
        var activePage = this.ApplicationDetails.ActivePage;     
        await activePage.RunAndWaitForRequestFinishedAsync(async () =>
        {
            await ProcessEntity(this);
        }, waitForRequestFinishedOptions);       
    }
}
