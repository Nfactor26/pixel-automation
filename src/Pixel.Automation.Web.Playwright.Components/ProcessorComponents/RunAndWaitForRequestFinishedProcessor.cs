using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components.Processors;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Pixel.Automation.Web.Playwright.Components;

[DataContract]
[Serializable]
[ToolBoxItem("Run And Wait For Request", "Playwright", "Run And Wait", iconSource: null, description: "Run and wait for request", tags: new string[] { "Run", "Wait", "Request", "Web" })]
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
    [AllowedTypes(typeof(string), typeof(Regex), typeof(Func<IRequest, bool>))]
    public Argument Predicate { get; set; } = new InArgument<Func<IRequest, bool>> { Mode = ArgumentMode.DataBound, CanChangeType = true };

    /// <summary>
    /// Optional input argument for <see cref="PageRunAndWaitForRequestOptions"/> that can be used to customize the run and wait for request operation
    /// </summary>
    [DataMember]
    [Display(Name = "Wait For Request Options", GroupName = "Configuration", Order = 20, Description = "Input argument for PageRunAndWaitForRequestOptions")]
    public Argument WaitForRequestOptions { get; set; } = new InArgument<PageRunAndWaitForRequestOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public RunAndWaitForRequestProcessor() : base("Run And Wait For Request", "RunAndWaitForRequest")
    {

    }

    public override async Task BeginProcessAsync()
    {
        var navigationOptions = this.WaitForRequestOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<PageRunAndWaitForRequestOptions>(this.WaitForRequestOptions) : null;
        var activePage = this.ApplicationDetails.ActivePage;
        switch (this.Predicate)
        {
            case InArgument<string>:
                var predicate = await this.ArgumentProcessor.GetValueAsync<string>(this.Predicate);
                await activePage.RunAndWaitForRequestAsync(async () =>
                {
                    await ProcessEntity(this);
                }, predicate, navigationOptions);
                break;
            case InArgument<Regex>:
                var regexPredicate = await this.ArgumentProcessor.GetValueAsync<Regex>(this.Predicate);
                await activePage.RunAndWaitForRequestAsync(async () =>
                {
                    await ProcessEntity(this);
                }, regexPredicate, navigationOptions);
                break;
            case InArgument<Func<IRequest, bool>>:
                var funcPredicate = await this.ArgumentProcessor.GetValueAsync<Func<IRequest, bool>>(this.Predicate);
                await activePage.RunAndWaitForRequestAsync(async () =>
                {
                    await ProcessEntity(this);
                }, funcPredicate, navigationOptions);
                break;
        }        
    }
}
