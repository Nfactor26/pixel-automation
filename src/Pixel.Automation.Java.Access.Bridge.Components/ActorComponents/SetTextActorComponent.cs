using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Set Text", "Java", iconSource: null, description: "Set text contents of a control", tags: new string[] { "Set Text", "Java" })]

    public class SetTextActorComponent : JABActorComponent
    {

        [DataMember]
        public Argument Input { get; set; } = new InArgument<string>();
      

        public SetTextActorComponent() : base("Set Text", "SetText")
        {

        }

        public override void Act()
        {
            AccessibleContextNode targetControl = this.GetTargetControl();
            var info = targetControl.GetInfo();
            if ((info.accessibleInterfaces & AccessibleInterfaces.cAccessibleTextInterface) != 0)
            {
                string textToSet = this.ArgumentProcessor.GetValue<string>(this.Input);
                targetControl.AccessBridge.Functions.SetTextContents(targetControl.JvmId, targetControl.AccessibleContextHandle, textToSet);
            }
            else
            {
                throw new InvalidOperationException($"Control doesn't support cAccessibleTextInterface.");
            }
        }
    }
}
