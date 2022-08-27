using FluentAssertions;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using System.Runtime.Serialization;

namespace Pixel.Automation.Assertions.Components;

/// <summary>
/// Assert that a <see cref="UIControl"/> is visible
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Is Visible", "Assertions", iconSource: null, description: "Assert that a UIControl is visible", tags: new string[] { "Visible", "Assert" })]
public class AssertIsVisibleActorComponent : AssertableControlActorComponent
{

    /// <summary>
    /// constructor
    /// </summary>
    public AssertIsVisibleActorComponent() : base("Assert Is Visible", "AssertIsVisible")
    {

    }

    public override async Task ActAsync()
    {
        var control = await this.GetTargetControl();
        bool isVisible = await control.IsVisibleAsync();
        isVisible.Should().BeTrue();
    }        
}
