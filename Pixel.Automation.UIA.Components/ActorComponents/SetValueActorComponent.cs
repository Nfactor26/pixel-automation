using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;
using System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Set Value", "UIA", iconSource: null, description: "Trigger Value pattern on AutomationElement to SetValue", tags: new string[] { "SetValue", "UIA" })]

    public class SetValueActorComponent : UIAActorComponent
    {
        [DataMember]     
        public Argument Input { get; set; } = new InArgument<string>();

        public SetValueActorComponent() : base("Set Value", "SetValue")
        {

        }

        public override void Act()
        {
            AutomationElement control = GetTargetControl();
            string inputForControl = ArgumentProcessor.GetValue<string>(this.Input);
            control.SetValue(inputForControl);
        }

    }
}
