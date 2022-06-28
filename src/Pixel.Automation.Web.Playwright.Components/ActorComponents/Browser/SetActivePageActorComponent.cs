using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components.ActorComponents;

/// <summary>
/// Use <see cref="SetActivePageActorComponent"/> to mark a page as an active page. All operations are performed on active page.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Set Active Page", "Playwright", "Browser", iconSource: null, description: "Navigate the browser to desired url", tags: new string[] { "goto", "navigate", "Web" })]
public class SetActivePageActorComponent : PlaywrightActorComponent
{   
    /// <summary>
    /// Input argument to provide the index of page to set as active page.
    /// </summary>
    [DataMember]
    [Display(Name = "Page Number", GroupName = "Configuration", Order = 20, Description = "Index (1 based) of the tab to set as active page")]
    public Argument PageNumber { get; set; } = new InArgument<int>() { DefaultValue = 2 };

    /// <summary>
    /// Constructor
    /// </summary>
    public SetActivePageActorComponent() : base("SetActivePage", "SetActivePage")
    {

    }

    /// <summary>
    /// Set specifed index as the active page.
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        int index = await ArgumentProcessor.GetValueAsync<int>(this.PageNumber);
        if (ApplicationDetails.ActiveContext.Pages.Count() >= index)
        {
            ApplicationDetails.ActivePage = ApplicationDetails.ActiveContext.Pages[index];
        }
        await Task.CompletedTask;
    }

    ///</inheritdoc>
    public override string ToString()
    {
        return "Set Active Page Actor";
    }
}
