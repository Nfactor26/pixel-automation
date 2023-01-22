using Pixel.Automation.Core.Attributes;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Use <see cref="GoBackActorComponent"/> to navigate the previous page in history.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Go Back", "Appium", "Browser", iconSource: null, description: "Navigate to the previous page in history", tags: new string[] { "go back", "back" })]
public class GoBackActorComponent : AppiumElementActorComponent
{
    /// <summary>
    /// Constructor
    /// </summary>
    public GoBackActorComponent() : base("Go Back", "GoBack")
    {

    }

    /// <summary>
    /// Navigate to the previous page in history
    /// </summary>
    public override async Task ActAsync()
    {
        this.ApplicationDetails.Driver.Navigate().Back();
        await Task.CompletedTask;
    }
}
