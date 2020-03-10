using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;
using System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Toggle", "UIA", iconSource: null, description: "Trigger Toggle pattern on AutomationElement", tags: new string[] { "Toggle", "UIA" })]
    public class ToggleActorComponent : UIAActorComponent
    {       
        public ToggleActorComponent() : base("Toggle","Toggle")
        {

        }

        public override void Act()
        {

            AutomationElement control = GetTargetControl();
            control.Toggle();
        }
        
    }
}
