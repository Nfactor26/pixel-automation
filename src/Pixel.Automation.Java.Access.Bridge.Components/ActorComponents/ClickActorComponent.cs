using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    /// <summary>
    /// Use <see cref="ClickActorComponent"/> to perform a click on a control
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Click", "Java", iconSource: null, description: "Perform click on control", tags: new string[] { "Click", "Java" })]
    public class ClickActorComponent : JABActorComponent
    {
        private readonly ILogger logger = Log.ForContext<ClickActorComponent>();
      
        /// <summary>
        /// Identifes the action to perform. Default action is click.
        /// </summary>
        [DataMember]
        [Display(Name = "Action", GroupName = "Configuration", Order = 10, Description = "Action to perform on control e.g. click, togglePoup, etc.")]
        public string Action { get; set; } = "click";
       
        /// <summary>
        /// Default constructor
        /// </summary>
        public ClickActorComponent() : base("Click", "Click")
        {

        }

        /// <summary>
        /// Perform the configured action on control.
        /// warning : Triggering an action which opens a modal dialog will throw error for subsequent actions on AccessibleWindow 
        /// consider simulating physical mouse clicks in these scenarios
        /// </summary>
        public override async Task ActAsync()
        {
            AccessibleContextNode targetControl = await this.GetTargetControl();
            var info = targetControl.GetInfo();
            if ((info.accessibleInterfaces & AccessibleInterfaces.cAccessibleActionInterface) != 0)
            {
                AccessibleActionsToDo actionsToDo = new AccessibleActionsToDo()
                {
                    actions = new AccessibleActionInfo[32],
                    actionsCount = 1
                };
                actionsToDo.actions[0] = new AccessibleActionInfo() { name = this.Action };
                targetControl.AccessBridge.Functions.DoAccessibleActions(targetControl.JvmId, targetControl.AccessibleContextHandle, ref actionsToDo, out int failure);
                logger.Information($"{this.Action} was performed on control with result {failure}.");
                return;
            }
            throw new InvalidOperationException($"Control: {this.TargetControl} doesn't support cAccessibleTextInterface.");
        }

    }
}
