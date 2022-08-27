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
[ToolBoxItem("Is Disabled", "Assertions", iconSource: null, description: "Assert that a UIControl is disabled", tags: new string[] { "Disabled", "Assert" })]
public class AssertIsDisabledActorComponent : AssertableControlActorComponent
{

    /// <summary>
    /// constructor
    /// </summary>
    public AssertIsDisabledActorComponent() : base("Assert Is Disabled", "AssertIsDisabled")
    {

    }

    public override async Task ActAsync()
    {
        var control = await this.GetTargetControl();
        bool isDisabled = await control.IsDisabledAsync();
        isDisabled.Should().BeTrue();
    }
}
