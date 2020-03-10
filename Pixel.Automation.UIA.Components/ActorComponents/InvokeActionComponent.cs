extern alias uiaComWrapper;
using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Invoke", "UIA", iconSource: null, description: "Trigger Invoke pattern on AutomationElement", tags: new string[] { "Invoke","UIA" })]
    public class InvokeActorComponent : UIAActorComponent
    {    
        public InvokeActorComponent():base("Invoke","Invoke")
        {

        }

        public override void Act()
        {
            AutomationElement control = GetTargetControl();
            control.Invoke();
        }
       
    }
}
