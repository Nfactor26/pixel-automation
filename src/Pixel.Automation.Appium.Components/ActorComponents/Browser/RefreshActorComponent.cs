using Pixel.Automation.Core.Attributes;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Use <see cref="RefreshActorComponent"/> to refresh the page.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Refresh", "Appium", "Browser", iconSource: null, description: "Refresh active page", tags: new string[] { "refresh" })]
public class RefreshActorComponent : AppiumElementActorComponent
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
        this.ApplicationDetails.Driver.Navigate().Refresh();
        await Task.CompletedTask;
    }
}
