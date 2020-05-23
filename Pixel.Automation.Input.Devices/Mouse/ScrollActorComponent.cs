using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Devices;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Input.Devices
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Scroll", "Input Device", "Mouse", iconSource: null, description: "Perform a scroll  action using mouse", tags: new string[] { "Scroll" })]

    public class ScrollActorComponent : DeviceInputActor
    {
        /// <summary>
        /// Note : 1 unit of scroll is equivalent to 120 pixels . This is specific to library being used
        /// </summary>
        [DataMember]
        [Description("Number of scroll unit to perform")]
        [Display(Name = "Amount", GroupName = "Scroll Configuration")]           
        public Argument ScrollAmount { get; set; } = new InArgument<int>() { DefaultValue = 1, CanChangeType = false };

        ScrollMode scrollMode = ScrollMode.Vertical;
        [DataMember]
        [Display(Name = "Mode", GroupName = "Scroll Configuration")]
        public ScrollMode ScrollMode
        {
            get => this.scrollMode;
            set
            {
                this.scrollMode = value;
                OnPropertyChanged();
                switch (value)
                {
                    case ScrollMode.Vertical:
                        this.ScrollDirection = ScrollDirection.Down;
                        break;
                    case ScrollMode.Horizontal:
                        this.ScrollDirection = ScrollDirection.Right;
                        break;
                }
                
            }
        }

        ScrollDirection scrollDirection = ScrollDirection.Down;
        [DataMember]
        [Display(Name = "Direction", GroupName = "Scroll Configuration")]
        public ScrollDirection ScrollDirection
        {
            get => this.scrollDirection;
            set
            {
                switch(this.scrollMode)
                {
                    case ScrollMode.Horizontal:
                        if (value == ScrollDirection.Left || value == ScrollDirection.Right)
                        {
                            this.scrollDirection = value;
                        }
                        break;
                    case ScrollMode.Vertical:
                        if (value == ScrollDirection.Down || value == ScrollDirection.Up)
                        {
                            this.scrollDirection = value;
                        }
                        break;
                }
                OnPropertyChanged();
            }
        }

        public ScrollActorComponent() : base("Scroll", "Scroll")
        {

        }

        public override void Act()
        {         
            int amountToScroll = this.ArgumentProcessor.GetValue<int>(this.ScrollAmount);
            if (this.scrollDirection.Equals(ScrollDirection.Down) || this.scrollDirection.Equals(ScrollDirection.Right))
            {
                amountToScroll *= -1;
            }
            var syntheticMouse = GetMouse();
            switch (this.scrollMode)
            {
                case ScrollMode.Horizontal:
                    syntheticMouse.HorizontalScroll(amountToScroll);
                    break;
                case ScrollMode.Vertical:
                    syntheticMouse.VerticalScroll(amountToScroll);
                    break;
            }
        }
    } 
}
