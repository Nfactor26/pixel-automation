using Pixel.Automation.Core;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components
{
    [Serializable]
    [DataContract]
    public class AnchorControl : NotifyPropertyChanged
    {
        [Display(Name = "Anchor Location", GroupName = "Control Details", Order = 10)]
        [Description("Location of the anchor control with reference to target control")]
        [DataMember]
        public AnchorLocation Location { get; set; }

        [Display(Name = "Target Control", GroupName = "Control Details", Order = 20)]
        [Description("Target Anchor Control")]
        [DataMember]
        public ControlEntity ControlDetails { get; set; }

        protected double xOffset = 0.0f;
        [DataMember]
        [Description("X offset from the anchor pivot point")]
        [Display(Name = "X-Offset", GroupName = "Offset", Order = 10)]
        public double XOffSet
        {
            get
            {
                return xOffset;
            }
            set
            {
                xOffset = value;
                OnPropertyChanged();
            }
        }

      
        protected double yOffset = 0.0f;
        [DataMember]
        [Description("Y offset from the anchor pivot point")]
        [Display(Name = "Y-Offset", GroupName = "Offset", Order = 20)]
        public double YOffSet
        {
            get
            {
                return yOffset;
            }
            set
            {
                yOffset = value;
                OnPropertyChanged();
            }
        }

        [NonSerialized]
        Rectangle boundingBox;
        public Rectangle BoundingBox
        {
            get => boundingBox;
            set => boundingBox = value;
        }
    }


    /// <summary>
    /// Indicates the position of anchor control with reference to the region of interest
    /// </summary>
    public enum AnchorLocation
    {
        Left,
        Right,
        Top,
        Bottom
    }
   
}
