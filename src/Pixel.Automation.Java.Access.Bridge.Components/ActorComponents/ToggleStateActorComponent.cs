using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    /// <summary>
    /// Use <see cref="ToggleStateActorComponent"/> to toggle the state of the control e.g. a select a node in tree control or unselect it by toggling it's existing state.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Toggle State", "Java", iconSource: null, description: "Toggle expand or popup behavior", tags: new string[] { "Toggle", "Java" })]
    public class ToggleStateActorComponent : JABActorComponent
    {
        private readonly ILogger logger = Log.ForContext<ToggleStateActorComponent>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public ToggleStateActorComponent() : base("Toggle State", "ToggleState")
        {

        }

        /// <summary>
        /// Toggle the state of control 
        /// </summary>
        public override async Task ActAsync()
        {
            var targetControl = await this.GetTargetControl();
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
                logger.Information("State of the control was toggled.");
                return;
            }
            throw new InvalidOperationException($"Control doesn't support cAccessibleTextInterface.");
        }

    }
}
