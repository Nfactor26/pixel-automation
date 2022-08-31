using Pixel.Automation.Core.Attributes;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// Use <see cref="RefreshActorComponent"/> to refresh the page.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Refresh", "Selenium", "Browser", iconSource: null, description: "Refresh active page", tags: new string[] { "Refresh", "Navigate", "Browser", "Web" })]
public class RefreshActorComponent : WebElementActorComponent
{   
    /// <summary>
    /// Constructor
    /// </summary>
    public RefreshActorComponent():base("Refresh", "Refresh")
    {

    }

    /// <summary>
    /// Navigate to the previous page in history
    /// </summary>
    public override async Task ActAsync()
    {
        this.ApplicationDetails.WebDriver.Navigate().Refresh();
        await Task.CompletedTask;
    }
}
