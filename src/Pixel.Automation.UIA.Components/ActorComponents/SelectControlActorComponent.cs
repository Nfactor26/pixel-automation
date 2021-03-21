extern alias uiaComWrapper;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.UIA.Components.Enums;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
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
        public SelectControlActorComponent() : base("Select")
        {

        }

        /// <summary>
        /// Select a control or add control to selection or remove control from selection based on the SelectionMode
        /// </summary>
        public override void Act()
        {
            AutomationElement control = GetTargetControl();
            switch(this.SelectionMode)
            {
                case SelectMode.Select:
                    control.Select();
                    logger.Information("Control was selected.");
                    break;
                case SelectMode.AddToSelection:
                    control.AddToSelection();
                    logger.Information("Control was added to selection.");
                    break;
                case SelectMode.RemoveFromSelection:
                    control.RemoveFromSelection();
                    logger.Information("Control was removed from selection");
                    break;
            }
        }

        public override string ToString()
        {
            return "Select Control Actor";
        }
    }
}
