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
[ToolBoxItem("Run And Wait For Request", "Playwright", "Run And Wait", iconSource: null, description: "Run and wait for request", tags: new string[] { "run", "wait", "request", "Web" })]
public class RunAndWaitForRequestProcessor : EntityProcessor
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
    /// Input argument to provide a predicate for matching request
    /// </summary>
    [DataMember]
    [Display(Name = "Predicate", GroupName = "Configuration", Order = 10, Description = "Input argumnet for predicate for matching request ")]
    public Argument Predicate { get; set; } = new InArgument<Func<IRequest, bool>> { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Optional input argument for <see cref="PageRunAndWaitForRequestOptions"/> that can be used to customize the run and wait for request operation
    /// </summary>
    [DataMember]
    [Display(Name = "Request Options", GroupName = "Configuration", Order = 20, Description = "Input argument for PageRunAndWaitForRequestOptions")]
    public Argument WaitForRequestOptions { get; set; } = new InArgument<PageRunAndWaitForRequestOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public RunAndWaitForRequestProcessor() : base("RunAndWaitForRequest", "RunAndWaitForRequest")
    {

    }

    public override async Task BeginProcessAsync()
    {
        var predicate = await this.ArgumentProcessor.GetValueAsync<Func<IRequest, bool>>(this.Predicate);
        var navigationOptions = this.WaitForRequestOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<PageRunAndWaitForRequestOptions>(this.WaitForRequestOptions) : null;
        var activePage = this.ApplicationDetails.ActivePage;
        await activePage.RunAndWaitForRequestAsync(async () =>
        {
            await ProcessEntity(this);
        }, predicate, navigationOptions);
    }
}
