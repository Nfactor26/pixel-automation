extern alias uiaComWrapper;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.Runtime.Serialization;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Add To Selection", "UIA", iconSource: null, description: "Trigger SelectionItemPattern pattern on AutomationElement to add it to group of selected items", tags: new string[] { "AddToSelection", "UIA" })]

    public class AddToSelectionActorComponent : UIAActorComponent
    {       

        public AddToSelectionActorComponent() : base("Add To Selection","AddToSelection")
        {

        }

        public override void Act()
        {
            AutomationElement control = GetTargetControl();
            control.AddToSelection();

            Log.Information("Control was selected");
        }
        
    }
}
