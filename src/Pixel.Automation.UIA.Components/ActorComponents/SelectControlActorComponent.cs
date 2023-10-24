using Pixel.Automation.Core.Attributes;
using Pixel.Automation.UIA.Components.Enums;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Pixel.Windows.Automation;

namespace Pixel.Automation.UIA.Components;

/// <summary>
/// Use <see cref="AddToSelectionActorComponent"/> to select a control.
/// AutomationElement must support <see cref="SelectionItemPattern"/>.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Select Control", "UIA", iconSource: null, description: "Trigger SelectionItemPattern pattern on AutomationElement to select it", tags: new string[] { "Select", "UIA" })]
public class SelectControlActorComponent : UIAActorComponent
{
    private readonly ILogger logger = Log.ForContext<SelectControlActorComponent>();

    /// <summary>
    /// Indicates whether the control should be selected, added to existing selection or removed from existing selection
    /// </summary>
    [DataMember]
    [Display(Name = "Selection Mode", GroupName = "Configuration", Order = 10, Description = "Indicates whether the control should be selected, added to existing selection or" +
        "removed from existing selection")]
    public SelectMode SelectionMode { get; set; } = SelectMode.Select;

    /// <summary>
    /// Default constructor
    /// </summary>
    public SelectControlActorComponent() : base("Select Control", "SelectControl")
    {

    }

    /// <summary>
    /// Select a control or add control to selection or remove control from selection based on the SelectionMode
    /// </summary>
    public override async Task ActAsync()
    {
        var (name, control) = await GetTargetControl();
        switch (this.SelectionMode)
        {
            case SelectMode.Select:
                control.Select();
                logger.Information("Control : '{0}' was selected", name);
                break;
            case SelectMode.AddToSelection:
                control.AddToSelection();
                logger.Information("Control : '{0}' was added to selection", name);
                break;
            case SelectMode.RemoveFromSelection:
                control.RemoveFromSelection();
                logger.Information("Control : '{0}' was removed from selection", name);
                break;
        }
    }

}
