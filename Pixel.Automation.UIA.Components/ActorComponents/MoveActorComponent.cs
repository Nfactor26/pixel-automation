using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;
using System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Move", "UIA", iconSource: null, description: "Trigger Transform pattern on AutomationElement to move it", tags: new string[] { "Move", "UIA" })]
    public class MoveActorComponent : UIAActorComponent
    {
        [DataMember]
        public Argument PosX { get; set; } = new InArgument<double>() { CanChangeMode = true, CanChangeType = false, DefaultValue = 0.0, Mode = ArgumentMode.Default };

        [DataMember]
        public Argument PosY { get; set; } = new InArgument<double>() { CanChangeMode = true, CanChangeType = false, DefaultValue = 0.0, Mode = ArgumentMode.Default };


        public MoveActorComponent() : base("Move","Move")
        {

        }       

        public override void Act()
        {
            AutomationElement control = GetTargetControl();
            double posX = this.ArgumentProcessor.GetValue<double>(this.PosX);
            double posY = this.ArgumentProcessor.GetValue<double>(this.PosY);
            control.MoveTo(posX, posY);
        }
               
    }
}
