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
    /// Use <see cref="ScrollViewActorComponent"/> to perform a horizontal and/or vertical scroll.
    /// Control must support <see cref="ScrollPattern"/>
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Scroll View", "UIA", iconSource: null, description: "Perform horizontal/vertical scroll", tags: new string[] { "Scroll", "UIA" })]
    public class ScrollViewActorComponent : UIAActorComponent
    {
        private readonly ILogger logger = Log.ForContext<ScrollViewActorComponent>();

        /// <summary>
        /// Indicates the amount by which horizontal scroll should be performed
        /// </summary>
        [DataMember]
        [Display(Name = "Horizontal Scroll %", GroupName = "Configuration", Order = 10, Description = "Indicates the horizontal scroll percent. Set to -1 for no change." )]
        public Argument HorizontalScrollAmount { get; set; } = new InArgument<double>() { DefaultValue = -1, Mode = ArgumentMode.Default };

        /// <summary>
        /// Indicates the amount by which vertical scroll should be performed
        /// </summary>
        [DataMember]
        [Display(Name = "Vertical Scroll %", GroupName = "Configuration", Order = 20, Description = "Indicates the vertical scroll perecent. Set to -1 for no change.")]
        public Argument VerticalScrollAmount { get; set; } = new InArgument<double>() { DefaultValue = -1, Mode = ArgumentMode.Default };

      
        /// <summary>
        /// Default constructor
        /// </summary>
        public ScrollViewActorComponent() : base("Scroll View", "ScrollView")
        {

        }

        /// <summary>
        /// Perform a horizontal and/or vertical scroll.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws InvalidOperationException if ScrollPattern is not supported</exception>      
        public override async Task ActAsync()
        {
            AutomationElement control = await GetTargetControl();
            double horizontalScrollAmount = Math.Clamp(await this.ArgumentProcessor.GetValueAsync<double>(this.HorizontalScrollAmount), -1 , 100);
            double verticalScrollAmount = Math.Clamp(await this.ArgumentProcessor.GetValueAsync<double>(this.VerticalScrollAmount), -1, 100);
            control.ScrollToPercent(horizontalScrollAmount, verticalScrollAmount);
            logger.Information($"Scroll set to {horizontalScrollAmount}%, {verticalScrollAmount}%");
        }
    }
}
