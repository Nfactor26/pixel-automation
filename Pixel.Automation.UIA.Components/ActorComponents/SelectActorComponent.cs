using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;
using System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Select", "UIA", iconSource: null, description: "Trigger SelectionItemPattern pattern on AutomationElement to select it", tags: new string[] { "Select", "UIA" })]

    public class SelectActorComponent : UIAActorComponent
    {
        public SelectActorComponent() : base("Select")
        {

        }

        public override void Act()
        {
            AutomationElement control = GetTargetControl();
            control.Select();
        }
    }
}
