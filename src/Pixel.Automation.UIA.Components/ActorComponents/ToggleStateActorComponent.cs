extern alias uiaComWrapper;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.Runtime.Serialization;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
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
        public ToggleStateActorComponent() : base("Toggle", "Toggle")
        {

        }

        /// <summary>
        /// Toggle the state of a control e.g. if a checkbox is checked, toggle action will uncheck it and vice-versa.
        /// </summary>
        public override void Act()
        {
            AutomationElement control = GetTargetControl();
            control.Toggle();
            logger.Information("State of the control was toggled.");
        }

        public override string ToString()
        {
            return "Toggle State Actor";
        }
    }
}
