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
[ToolBoxItem("Run And Wait For Console Message", "Playwright", "Run And Wait", iconSource: null, description: "Run and wait for console message", tags: new string[] { "run", "wait", "console", "Web" })]
public class RunAndWaitForConsoleMessageProcessor : EntityProcessor
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
    /// Optional input argument for <see cref="PageRunAndWaitForConsoleMessageOptions"/> that can be used to customize the run and wait for file console message operation
    /// </summary>
    [DataMember]
    [Display(Name = "Wait For Console Message Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Argument to configure PageRunAndWaitForConsoleMessageOptions")]
    public Argument ConsoleMessageOptions { get; set; } = new InArgument<PageRunAndWaitForConsoleMessageOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public RunAndWaitForConsoleMessageProcessor() : base("Run And Wait For Console Message", "RunAndWaitForConsoleMessage")
    {

    }

    public override async Task BeginProcessAsync()
    {
        var consoleMessageOptions = this.ConsoleMessageOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<PageRunAndWaitForConsoleMessageOptions>(this.ConsoleMessageOptions) : null;
        var activePage = this.ApplicationDetails.ActivePage;
        await activePage.RunAndWaitForConsoleMessageAsync(async () =>
        {
            await ProcessEntity(this);
        }, consoleMessageOptions);
    }
}
