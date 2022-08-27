using FluentAssertions;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using System.Runtime.Serialization;

namespace Pixel.Automation.Assertions.Components;

/// <summary>
/// Assert that a <see cref="UIControl"/> is enabled.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Is Enabled", "Assertions", iconSource: null, description: "Assert that a UIControl is enabled", tags: new string[] { "Enabled", "Assert" })]
public class AssertIsEnabledActorComponent : AssertableControlActorComponent
{

    /// <summary>
    /// constructor
    /// </summary>
    public AssertIsEnabledActorComponent() : base("Assert Is Enabled", "AssertIsEnabled")
    {

    }

    public override async Task ActAsync()
    {
        var control = await this.GetTargetControl();
        bool isEnabled = await control.IsEnabledAsync();
        isEnabled.Should().BeTrue();
    }
}
