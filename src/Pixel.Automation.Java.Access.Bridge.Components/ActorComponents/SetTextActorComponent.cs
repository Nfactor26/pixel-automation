using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    /// <summary>
    /// Use <see cref="SetTextActorComponent"/> to set the text of a control
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Set Text", "Java", iconSource: null, description: "Set text contents of a control", tags: new string[] { "Set Text", "Java" })]
    public class SetTextActorComponent : JABActorComponent
    {
        private readonly ILogger logger = Log.ForContext<SelectListItemActorComponent>();

        /// <summary>
        /// Value of text to set on control
        /// </summary>
        [DataMember]
        [Display(Name = "Input", GroupName = "Input", Order = 10, Description = "Value of text to be set on control")]
        public Argument Input { get; set; } = new InArgument<string>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

        /// <summary>
        /// Default constructor
        /// </summary>
        public SetTextActorComponent() : base("Set Text", "SetText")
        {

        }

        /// <summary>
        /// Set the text of a control.
        /// </summary>
        public override void Act()
        {
            AccessibleContextNode targetControl = this.GetTargetControl();
            var info = targetControl.GetInfo();
            if ((info.accessibleInterfaces & AccessibleInterfaces.cAccessibleTextInterface) != 0)
            {
                string textToSet = this.ArgumentProcessor.GetValue<string>(this.Input);
                targetControl.AccessBridge.Functions.SetTextContents(targetControl.JvmId, targetControl.AccessibleContextHandle, textToSet);
                logger.Information("Text was set on control.");
                return;
            }
            throw new InvalidOperationException($"Control doesn't support cAccessibleTextInterface.");
        }

        public override string ToString()
        {
            return "Set Text Actor";
        }
    }
}
