using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;
using System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Set Focus", "UIA", iconSource: null, description: "Set Focus on automation element", tags: new string[] { "Focus", "UIA" })]

    public class SetFocusActorComponent : UIAActorComponent
    {        
        public SetFocusActorComponent() : base("Set Focus","SetFocus")
        {

        }

        public override void Act()
        {
            AutomationElement control = GetTargetControl();
            control.SetFocus();
        }

    }
}
