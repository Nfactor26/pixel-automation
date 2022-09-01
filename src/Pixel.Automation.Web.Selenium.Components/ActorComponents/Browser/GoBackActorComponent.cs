using Pixel.Automation.Core.Attributes;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// Use <see cref="GoBackActorComponent"/> to navigate the previous page in history.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Go Back", "Selenium", "Browser", iconSource: null, description: "Navigate to the previous page in history", tags: new string[] { "GoBack", "Back", "Navigate", "Browser", "Web" })]
public class GoBackActorComponent : WebElementActorComponent
{   
    /// <summary>
    /// Constructor
    /// </summary>
    public GoBackActorComponent():base("Go Back", "GoBack")
    {

    }

    /// <summary>
    /// Navigate to the previous page in history
    /// </summary>
    public override async Task ActAsync()
    {
        this.ApplicationDetails.WebDriver.Navigate().Back();
        await Task.CompletedTask;
    }
}
