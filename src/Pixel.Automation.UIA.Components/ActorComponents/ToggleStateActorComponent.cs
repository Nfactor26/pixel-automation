using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.UIA.Components;

/// <summary>
/// Use <see cref="ToggleStateActorComponent"/> to toggle the state of a control e.g. checkbox
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Toggle State", "UIA", iconSource: null, description: "Trigger Toggle pattern on AutomationElement", tags: new string[] { "Toggle", "UIA" })]
public class ToggleStateActorComponent : UIAActorComponent
{
    private readonly ILogger logger = Log.ForContext<ToggleStateActorComponent>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public ToggleStateActorComponent() : base("Toggle State", "ToggleState")
    {

    }

    /// <summary>
    /// Toggle the state of a control e.g. if a checkbox is checked, toggle action will uncheck it and vice-versa.
    /// </summary>
    public override async Task ActAsync()
    {
        var (name, control) = await GetTargetControl();
        control.Toggle();
        logger.Information("State of the control : '{0}' was toggled.", name);
    }

}
