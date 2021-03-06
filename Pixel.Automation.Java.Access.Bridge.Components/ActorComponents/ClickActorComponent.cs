using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Click", "Java", iconSource: null, description: "Perform click on control", tags: new string[] { "Click", "Java" })]
    public class ClickActorComponent : JABActorComponent
    {
        string defaultAction = "click";
        [DataMember]      
        public string DefaultAction
        {
            get
            {
                return defaultAction;
            }
            set
            {
                defaultAction = value;
            }
        }
        public ClickActorComponent():base("Click","Click")
        {

        }

        /// <summary>
        /// warning : Triggering an action which opens a modal dialog will throw error for subsequent actions on AccessibleWindow 
        /// consider simulating physical mouse clicks in these scenarios
        /// </summary>
        public override void Act()
        {
            AccessibleContextNode targetControl = this.GetTargetControl();
            var info = targetControl.GetInfo();
            if ((info.accessibleInterfaces & AccessibleInterfaces.cAccessibleActionInterface) != 0)
            {
                AccessibleActionsToDo actionsToDo = new AccessibleActionsToDo()
                {
                    actions = new AccessibleActionInfo[32],
                    actionsCount = 1
                };
                actionsToDo.actions[0] = new AccessibleActionInfo() { name = defaultAction };
                targetControl.AccessBridge.Functions.DoAccessibleActions(targetControl.JvmId, targetControl.AccessibleContextHandle, ref actionsToDo, out int failure);
            }          

        }
    }
}
