extern alias uiaComWrapper;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Runtime.Serialization;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    /// <summary>
    /// Use <see cref="MoveActorComponent"/> to move a control to new position.
    /// Control must support <see cref="TransformPattern"/>
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Move", "UIA", "Transform", iconSource: null, description: "Trigger Transform pattern on AutomationElement to move it", tags: new string[] { "Move", "UIA" })]
    public class MoveActorComponent : UIAActorComponent
    {
        private readonly ILogger logger = Log.ForContext<MoveActorComponent>();

        /// <summary>
        /// New Position co-ordinates of the control
        /// </summary>
        [DataMember]
        [Display(Name = "Position", GroupName = "Configuration", Order = 10, Description = "New position co-ordinates of the control")]
        public Argument Position { get; set; } = new InArgument<Point>() { CanChangeMode = true, CanChangeType = false, DefaultValue = new Point(), Mode = ArgumentMode.Default };

       /// <summary>
       /// Default constructor
       /// </summary>
        public MoveActorComponent() : base("Move", "Move")
        {

        }

        /// <summary>
        /// Move the control e.g. window or a dialog to a new position 
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws InvalidOperationException if TransformPattern is not supported</exception>      
        public override void Act()
        {
            AutomationElement control = GetTargetControl();
            var newPosition = this.ArgumentProcessor.GetValue<Point>(this.Position);           
            control.MoveTo(newPosition.X, newPosition.Y);
            logger.Information($"Control was moved to position : ({newPosition.X}, {newPosition.Y})");
        }

        public override string ToString()
        {
            return "Move Actor";
        }
    }
}
