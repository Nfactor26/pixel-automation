extern alias uiaComWrapper;
using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Collapse", "UIA", iconSource: null, description: "Trigger ExpandCollapsePattern on AutomationElement to collapse it", tags: new string[] { "Collapse", "UIA" })]

    public class CollapseActorComponent : UIAActorComponent
    {        
        public CollapseActorComponent() : base("Collapse","Collapse")
        {

        }

        public override void Act()
        {
            AutomationElement control = GetTargetControl();
            control.Collapse();
        }
               
    }
}
