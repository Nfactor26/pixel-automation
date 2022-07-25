extern alias uiaComWrapper;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    /// <summary>
    /// Use <see cref="ResizeActorComponent"/> to resize a control.
    /// Control must support <see cref="TransformPattern"/>
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Resize", "UIA", "Transform", iconSource: null, description: "Trigger Transform pattern on AutomationElement to resize", tags: new string[] { "Resize", "UIA" })]
    public class ResizeActorComponent : UIAActorComponent
    {
        private readonly ILogger logger = Log.ForContext<ResizeActorComponent>();

        /// <summary>
        /// New value of the width
        /// </summary>
        [DataMember]
        [Display(Name = "Width", GroupName = "Configuration", Order = 10, Description = "New value of width")]
        public Argument Width { get; set; } = new InArgument<double>() { Mode = ArgumentMode.Default,  DefaultValue = 0.0 };

        /// <summary>
        /// New value of the height
        /// </summary>
        [DataMember]
        [Display(Name = "Height", GroupName = "Configuration", Order = 20, Description = "New value of height")]
        public Argument Height { get; set; } = new InArgument<double>() { Mode = ArgumentMode.Default, DefaultValue = 0.0};

        /// <summary>
        /// Default constructor
        /// </summary>
        public ResizeActorComponent() : base("Resize", "Resize")
        {

        }

        /// <summary>
        /// Resize the control e.g. a window
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws InvalidOperationException if TransformPattern is not supported</exception>      
        public override async Task ActAsync()
        {
            AutomationElement control = await GetTargetControl();
            double width = await this.ArgumentProcessor.GetValueAsync<double>(this.Width);
            double height = await this.ArgumentProcessor.GetValueAsync<double>(this.Height);
            control.ResizeTo(width, height);
            logger.Information($"Control was resized to ({width}, {height})");
        }

        public override string ToString()
        {
            return "Resize Actor";
        }
    }
}
