extern alias uiaComWrapper;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    /// <summary>
    /// Use <see cref="SetValueActorComponent"/> to set the value of a control.
    /// Control must support <see cref="ValuePattern"/>.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Set Value", "UIA", iconSource: null, description: "Trigger Value pattern on AutomationElement to SetValue", tags: new string[] { "SetValue", "UIA" })]
    public class SetValueActorComponent : UIAActorComponent
    {
        private readonly ILogger logger = Log.ForContext<SetValueActorComponent>();

        /// <summary>
        /// Value to be set on control
        /// </summary>
        [DataMember]
        [Display(Name = "Input", GroupName = "Input", Order = 10, Description = "Value to set on control")]
        public Argument Input { get; set; } = new InArgument<string>() { DefaultValue = string.Empty, Mode = ArgumentMode.Default };

        /// <summary>
        /// Default constructor
        /// </summary>
        public SetValueActorComponent() : base("Set Value", "SetValue")
        {

        }

        /// <summary>
        /// Set value on a control.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws InvalidOperationException if ValuePattern is not supported</exception>      
        public override void Act()
        {
            AutomationElement control = GetTargetControl();
            string inputForControl = ArgumentProcessor.GetValue<string>(this.Input);
            control.SetValue(inputForControl);
            logger.Information("Value was set on control.");
        }

        public override string ToString()
        {
            return "Set Value Actor";
        }
    }
}
