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
[ToolBoxItem("Run And Wait For Response", "Playwright", "Run And Wait", iconSource: null, description: "Run and wait for response", tags: new string[] { "run", "wait", "response", "Web" })]
public class RunAndWaitForResponseProcessor : EntityProcessor
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
    /// Input argument to provide predicate for response url matching
    /// </summary>
    [DataMember]
    [Display(Name = "Predicate", GroupName = "Configuration", Order = 20, Description = "Input argument for predicate for response url matching")]
    public Argument Predicate { get; set; } = new InArgument<Func<IResponse, bool>> { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Optional input argument for <see cref="PageRunAndWaitForResponseOptions"/> that can be used to customize the run and wait for response operation
    /// </summary>
    [DataMember]
    [Display(Name = "Response Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for WaitForResponseOptions")]
    public Argument WaitForResponseOptions { get; set; } = new InArgument<PageRunAndWaitForResponseOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public RunAndWaitForResponseProcessor() : base("RunAndWaitForResponse", "RunAndWaitForResponse")
    {

    }

    public override async Task BeginProcessAsync()
    {
        var predicate = await this.ArgumentProcessor.GetValueAsync<Func<IResponse, bool>>(this.Predicate);
        var waitForResponseOptions = this.WaitForResponseOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<PageRunAndWaitForResponseOptions>(this.WaitForResponseOptions) : null;
        var activePage = this.ApplicationDetails.ActivePage;
        await activePage.RunAndWaitForResponseAsync(async () =>
        {
            await ProcessEntity(this);
        }, predicate, waitForResponseOptions);
    }
}
