using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;
using System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Remove From Selection", "UIA", iconSource: null, description: "Trigger SelectionItemPattern pattern on AutomationElement to remove it from group of selected items", tags: new string[] { "RemoveFromSelection", "UIA" })]
    public class RemoveFromSelectionActionComponent : UIAActorComponent
    {
        
        public RemoveFromSelectionActionComponent() : base("Remove From Selection","RemoveFromSelectionAction")
        {

        }

        public override void Act()
        {
            AutomationElement control = GetTargetControl();
            control.RemoveFromSelection();
        }
       
    }
}
