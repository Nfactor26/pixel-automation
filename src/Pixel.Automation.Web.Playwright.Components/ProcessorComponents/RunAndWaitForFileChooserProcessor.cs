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
[ToolBoxItem("Run And Wait For File Chooser", "Playwright", "Run And Wait", iconSource: null, description: "Run and wait for file chooser", tags: new string[] { "run", "wait", "file", "Web" })]
public class RunAndWaitForFileChooserProcessor : EntityProcessor
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
    /// Optional input argument for <see cref="PageRunAndWaitForFileChooserOptions"/> that can be used to customize the run and wait for file chooser operation
    /// </summary>
    [DataMember]
    [Display(Name = "Wait For File Chooser Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for PageRunAndWaitForFileChooserOptions")]
    public Argument FileChooserOptions { get; set; } = new InArgument<PageRunAndWaitForFileChooserOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public RunAndWaitForFileChooserProcessor() : base(" Run And Wait For File Chooser", " RunAndWaitForFileChooser")
    {

    }

    public override async Task BeginProcessAsync()
    {
        var fileChooserOptions = this.FileChooserOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<PageRunAndWaitForFileChooserOptions>(this.FileChooserOptions) : null;
        var activePage = this.ApplicationDetails.ActivePage;
        await activePage.RunAndWaitForFileChooserAsync(async () =>
        {
            await ProcessEntity(this);
        }, fileChooserOptions);
    }
}
