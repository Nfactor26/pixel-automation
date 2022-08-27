using FluentAssertions;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using System.Runtime.Serialization;

namespace Pixel.Automation.Assertions.Components;

/// <summary>
/// Assert that a <see cref="UIControl"/> is checked.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Is Checked", "Assertions", iconSource: null, description: "Assert that a UIControl is checked", tags: new string[] { "Checked", "Assert" })]
public class AssertIsCheckedActorComponent : AssertableControlActorComponent
{

    /// <summary>
    /// constructor
    /// </summary>
    public AssertIsCheckedActorComponent() : base("Assert Is Checked", "AssertIsChecked")
    {

    }

    public override async Task ActAsync()
    {
        var control = await this.GetTargetControl();
        bool isChecked = await control.IsCheckedAsync();
        isChecked.Should().BeTrue();
    }
}
