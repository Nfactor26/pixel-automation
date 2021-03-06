using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Get Value", "Java", iconSource: null, description: "If the located control supports AccessibleValueInterface, get the current value using this interface", tags: new string[] { "GetValue", "UIA" })]

    public class GetValueActorComponent : JABActorComponent
    {
        [DataMember]
        [Description("Store the value in Result Argument")]
        public Argument Result { get; set; } = new OutArgument<string>();

        public GetValueActorComponent() : base("Get Value", "GetValue")
        {

        }

        public override void Act()
        {
            AccessibleContextNode targetControl = GetTargetControl();
            var info = targetControl.GetInfo();
            if ((info.accessibleInterfaces & AccessibleInterfaces.cAccessibleValueInterface) != 0)
            {
                var sb = new StringBuilder(1024);
                targetControl.AccessBridge.Functions.GetCurrentAccessibleValueFromContext(targetControl.JvmId, targetControl.AccessibleContextHandle, sb, (short)sb.Capacity);
                string controlValue = sb.ToString();
                this.ArgumentProcessor.SetValue<string>(this.Result, controlValue);
                return;
            }
            throw new InvalidOperationException($"Control: {this.TargetControl} doesn't support cAccessibleValueInterface.");
        }
    }
}
