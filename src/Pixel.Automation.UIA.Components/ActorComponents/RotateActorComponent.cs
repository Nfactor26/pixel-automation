extern alias uiaComWrapper;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using uiaComWrapper::System.Windows.Automation;


namespace Pixel.Automation.UIA.Components
{
    /// <summary>
    /// Use <see cref="RotateActorComponent"/> to rotate the control by specified amount.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Rotate", "UIA", "Transform", iconSource: null, description: "Trigger Invoke pattern on AutomationElement", tags: new string[] { "Invoke", "UIA" })]
    public class RotateActorComponent : UIAActorComponent
    {
        private readonly ILogger logger = Log.ForContext<RotateActorComponent>();

        /// <summary>
        /// The number of degrees to rotate the element. A positive number rotates clockwise; a negative number rotates counterclockwise.
        /// </summary>
        [DataMember]
        [Display(Name = "Rotate By", GroupName = "Configuration", Order = 10, Description = "The number of degrees to rotate the element." +
            "A positive number rotates clockwise; a negative number rotates counterclockwise.")]
        public Argument RotateBy { get; set; } = new InArgument<double>() { CanChangeType = false, DefaultValue = 0.0, Mode = ArgumentMode.Default };

        /// <summary>
        /// Default constructor
        /// </summary>
        public RotateActorComponent() : base("Rotate", "Rotate")
        {

        }

        /// <summary>
        /// Rotate the control by a given amount.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws InvalidOperationException if TransformPattern is not supported</exception>      
        public override async Task ActAsync()
        {
            AutomationElement control = await GetTargetControl();
            double rotateBy = await this.ArgumentProcessor.GetValueAsync<double>(this.RotateBy);         
            control.RotateBy(rotateBy);
            logger.Information($"Control was rotated by {rotateBy} degrees.");
        }

    }
}
