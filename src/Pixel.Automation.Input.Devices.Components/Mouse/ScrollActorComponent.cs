using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Devices;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Input.Devices.Components
{
    /// <summary>
    /// Use <see cref="ScrollActorComponent"/> to perform a horizontal or vertical scroll.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Scroll", "Input Device", "Mouse", iconSource: null, description: "Perform a scroll  action using mouse", tags: new string[] { "Scroll" })]
    public class ScrollActorComponent : InputDeviceActor
    {
        private readonly ILogger logger = Log.ForContext<ScrollActorComponent>();

        /// <summary>
        /// Indicates whether to scroll left or right if scroll mode is horizontal and up or down if scroll mode is vertical.
        /// </summary>
        [DataMember]
        [Display(Name = "Direction", GroupName = "Configuration", Order = 10, Description = "Direction of scroll")]
        public ScrollDirection ScrollDirection { get; set; } = ScrollDirection.Down;

        /// <summary>
        /// Note : 1 unit of scroll is equivalent to 120 pixels . This is specific to library being used
        /// </summary>
        [DataMember]
        [Display(Name = "Amount", GroupName = "Configuration", Order = 20, Description = "Amount of scroll. 1 unit equals 120 pixel.")]
        public Argument ScrollAmount { get; set; } = new InArgument<int>() { DefaultValue = 1, CanChangeType = false };

        /// <summary>
        /// Default constructor
        /// </summary>
        public ScrollActorComponent() : base("Scroll", "Scroll")
        {

        }

        /// <summary>
        /// Perform a horizontal or vertical scroll
        /// </summary>
        public override async Task ActAsync()
        {
            int amountToScroll = await this.ArgumentProcessor.GetValueAsync<int>(this.ScrollAmount);
            if (this.ScrollDirection.Equals(ScrollDirection.Down) || this.ScrollDirection.Equals(ScrollDirection.Right))
            {
                amountToScroll *= -1;
            }
            var syntheticMouse = GetMouse();
            switch (this.ScrollDirection)
            {
                case ScrollDirection.Left:
                case ScrollDirection.Right:
                    syntheticMouse.HorizontalScroll(amountToScroll);
                    logger.Information($"Mouse was horizontally scrolled by {amountToScroll} ticks in direction {this.ScrollDirection}");
                    break;
                case ScrollDirection.Up:
                case ScrollDirection.Down:
                    syntheticMouse.VerticalScroll(amountToScroll);
                    logger.Information($"Mouse was vertically scrolled by {amountToScroll} ticks in direction {this.ScrollDirection}");
                    break;
            }
            await Task.CompletedTask;
        }

        public override string ToString()
        {
            return "Scroll Actor";
        }
    }
}
