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
[ToolBoxItem("Is Hidden", "Assertions", iconSource: null, description: "Assert that a UIControl is hidden", tags: new string[] { "Hidden", "Assert" })]
public class AssertIsHiddenActorComponent : AssertableControlActorComponent
{

    /// <summary>
    /// constructor
    /// </summary>
    public AssertIsHiddenActorComponent() : base("Assert Is Hidden", "AssertIsHidden")
    {

    }

    public override async Task ActAsync()
    {
        var control = await this.GetTargetControl();
        bool isHidden = await control.IsHiddenAsync();
        isHidden.Should().BeTrue();
    }
}
