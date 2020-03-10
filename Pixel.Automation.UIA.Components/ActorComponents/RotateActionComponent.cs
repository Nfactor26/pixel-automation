using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;
using System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Rotate", "UIA", iconSource: null, description: "Trigger Invoke pattern on AutomationElement", tags: new string[] { "Invoke", "UIA" })]
    public class RotateActorComponent : UIAActorComponent
    {
        [DataMember]
        public Argument RotateBy { get; set; } = new InArgument<double>() { CanChangeMode = true, CanChangeType = false, DefaultValue = 0.0, Mode = ArgumentMode.Default };

        public RotateActorComponent() : base("Rotate","Rotate")
        {

        }       

        public override void Act()
        {
            AutomationElement control = GetTargetControl();
            double rotateBy = this.ArgumentProcessor.GetValue<double>(this.RotateBy);         
            control.RotateBy(rotateBy);
        }
       
    }
}
