﻿using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.UIA.Components;

/// <summary>
/// Use <see cref="SetFocusActorComponent"/> to set focus on a control.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Set Focus", "UIA", iconSource: null, description: "Set Focus on automation element", tags: new string[] { "Focus", "UIA" })]
public class SetFocusActorComponent : UIAActorComponent
{
    private readonly ILogger logger = Log.ForContext<SetFocusActorComponent>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public SetFocusActorComponent() : base("Set Focus", "SetFocus")
    {

    }

    /// <summary>
    /// Set focus on a control
    /// </summary>
    public override async Task ActAsync()
    {
        var (name, control) = await GetTargetControl();
        control.SetFocus();
        logger.Information("Focus was set on control ; '{0}'", name);
    }

}
