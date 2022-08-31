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
[ToolBoxItem("Run And Wait For Popup", "Playwright", "Run And Wait", iconSource: null, description: "Run and wait for popup", tags: new string[] { "Run", "Wait", "Popup", "Web" })]
public class RunAndWaitForPopupProcessor : EntityProcessor
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
    /// Optional input argument for <see cref="PageRunAndWaitForPopupOptions"/> that can be used to customize the run and wait for popup operation
    /// </summary>
    [DataMember]
    [Display(Name = "Popup Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for PageRunAndWaitForPopup operation")]
    public Argument PopupOptions { get; set; } = new InArgument<PageRunAndWaitForPopupOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public RunAndWaitForPopupProcessor() : base("Run And Wait Fo rPopup", "RunAndWaitForPopup")
    {

    }

    public override async Task BeginProcessAsync()
    {
        var popupOptions = this.PopupOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<PageRunAndWaitForPopupOptions>(this.PopupOptions) : null;
        var activePage = this.ApplicationDetails.ActivePage;
        await activePage.RunAndWaitForPopupAsync(async () =>
        {
            await ProcessEntity(this);
        }, popupOptions);
    }
}
