using Pixel.Automation.Core.Attributes;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Use <see cref="GoForwardActorComponent"/> to navigate to the next page in history.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Go Forward", "Appium", "Browser", iconSource: null, description: "Navigate to the next page in history", tags: new string[] { "forward", "go" })]
public class GoForwardActorComponent : AppiumElementActorComponent
{        
    /// <summary>
    /// Constructor
    /// </summary>
    public GoForwardActorComponent():base("Go Forward", "GoForward")
    {

    }

    /// <summary>
    /// Navigate to the next page in history
    /// </summary>
    public override async Task ActAsync()
    {
        this.ApplicationDetails.Driver.Navigate().Forward();
        await Task.CompletedTask;
    }
}
