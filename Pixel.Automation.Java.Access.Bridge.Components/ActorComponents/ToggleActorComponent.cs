using Pixel.Automation.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components.ActorComponents
{

    [DataContract]
    [Serializable]
    [ToolBoxItem("Toggle", "Java", iconSource: null, description: "Toggle expand or popup behavior", tags: new string[] { "Toggle", "Java" })]
    public class ToggleActorComponent : JABActorComponent
    {
        public ToggleActorComponent() : base("Toggle", "Toggle")
        {

        }

        public override void Act()
        {
            var targetControl = this.GetTargetControl();
            var info = targetControl.GetInfo();
            AccessibleActionInfo actionToPerform = default;
            if ((info.accessibleInterfaces & AccessibleInterfaces.cAccessibleActionInterface) != 0)
            {
                targetControl.AccessBridge.Functions.GetAccessibleActions(targetControl.JvmId, targetControl.AccessibleContextHandle, out AccessibleActions accessibleActions);
                foreach(var action in accessibleActions.actionInfo)
                {
                    if(action.name.Equals("togglePopup") || action.name.Equals("toggleexpand"))
                    {
                        actionToPerform = action;
                        break;
                    }                  
                }
                if(!string.IsNullOrEmpty(actionToPerform.name))
                {
                    AccessibleActionsToDo actionsToDo = new AccessibleActionsToDo()
                    {
                        actions = new AccessibleActionInfo[32],
                        actionsCount = 1
                    };
                    actionsToDo.actions[0] = actionToPerform;
                    targetControl.AccessBridge.Functions.DoAccessibleActions(targetControl.JvmId, targetControl.AccessibleContextHandle, ref actionsToDo, out int failure);
                }                
            }
        }

    }
}
