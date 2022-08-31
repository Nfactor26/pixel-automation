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
[ToolBoxItem("Run And Wait For Navigation", "Playwright", "Run And Wait", iconSource: null, description: "Run and wait for navigation", tags: new string[] { "Run", "Wait", "Navigation", "Web" })]
public class RunAndWaitForNavigationProcessor : EntityProcessor
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
    /// Optional input argument for <see cref="PageRunAndWaitForNavigationOptions"/> that can be used to customize the run and wait for navigation operation
    /// </summary>
    [DataMember]
    [Display(Name = "Wait For Navigation Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for PageRunAndWaitForNavigationOptions")]
    public Argument NavigationOptions { get; set; } = new InArgument<PageRunAndWaitForNavigationOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    ///C onstructor
    /// </summary>
    public RunAndWaitForNavigationProcessor() : base("Run And Wait For Navigation", "RunAndWaitForNavigation")
    {

    }

    public override async Task BeginProcessAsync()
    {
        var navigationOptions = this.NavigationOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<PageRunAndWaitForNavigationOptions>(this.NavigationOptions) : null;
        var activePage = this.ApplicationDetails.ActivePage;
        await activePage.RunAndWaitForNavigationAsync(async () =>
        {
            await ProcessEntity(this);
        }, navigationOptions);
    }
}
