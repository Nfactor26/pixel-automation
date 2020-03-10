extern alias uiaComWrapper;
using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Expand", "UIA", iconSource: null, description: "Trigger ExpandCollapsePattern pattern on AutomationElement to expand it", tags: new string[] { "Expand", "UIA" })]
    public class ExpandActorComponent : UIAActorComponent
    {
       
        public ExpandActorComponent() : base("Expand","Expand")
        {

        }

        public override void Act()
        {
            AutomationElement control = GetTargetControl();
            control.Expand();
        }
        
    }
}
