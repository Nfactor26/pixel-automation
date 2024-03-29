﻿using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Pixel.Windows.Automation;

namespace Pixel.Automation.UIA.Components;

/// <summary>
/// Use <see cref="CollapseActorComponent"/> to collapse the state of a control.
/// Control must support <see cref="ExpandCollapsePattern"/>
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Collapse", "UIA", iconSource: null, description: "Trigger ExpandCollapsePattern on AutomationElement to collapse it", tags: new string[] { "Collapse", "UIA" })]
public class CollapseActorComponent : UIAActorComponent
{
    private readonly ILogger logger = Log.ForContext<CollapseActorComponent>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public CollapseActorComponent() : base("Collapse", "Collapse")
    {

    }

    /// <summary>
    /// Collapse the state of control that supports ExpandCollasePattern.
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws InvalidOperationException if ExpandCollapsePattern is not supported</exception>
    public override async Task ActAsync()
    {
        var (name, control) = await GetTargetControl();
        if(control.IsControlCollapsed())
        {
            logger.Information("Control : '{0}' is already collapsed", name);
        }
        control.Collapse();
        logger.Information("Control : '{0}' was collapsed", name);
    }

}
