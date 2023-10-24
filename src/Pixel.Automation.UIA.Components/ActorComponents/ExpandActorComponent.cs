using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Pixel.Windows.Automation;

namespace Pixel.Automation.UIA.Components; 

/// <summary>
/// Use <see cref="ExpandActorComponent"/> to expand the state of a control.
/// Control must support <see cref="ExpandCollapsePattern"/>.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Expand", "UIA", iconSource: null, description: "Trigger ExpandCollapsePattern pattern on AutomationElement to expand it", tags: new string[] { "Expand", "UIA" })]
public class ExpandActorComponent : UIAActorComponent
{
    private readonly ILogger logger = Log.ForContext<ExpandActorComponent>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public ExpandActorComponent() : base("Expand", "Expand")
    {

    }

    /// <summary>
    /// Expand the state of  control that supports ExpandCollasePattern.
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws InvalidOperationException if ExpandCollapsePattern is not supported</exception>
    public override async Task ActAsync()
    {
        var (name, control) = await GetTargetControl();
        if(control.IsControlExpanded())
        {
            logger.Information("Control : '{0}' is already expanded", name);
            return;
        }
        control.Expand();
        logger.Information("Control : '{0}' was expanded", name);
    }

}
