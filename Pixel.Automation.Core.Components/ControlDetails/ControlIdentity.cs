using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components
{
    [DataContract]
    [Serializable]
    public abstract class ControlIdentity : NotifyPropertyChanged, IControlIdentity , ICloneable
    {
        #region  General 

        [DataMember]
        [Browsable(false)]
        public string ApplicationId { get; set; }
  
        [DataMember]
        public string Name { get; set; }

        [DataMember]        
        [Description("Name of the source image")]      
        [Browsable(false)]
        public string ControlImage { get; set; }

        protected Rectangle boundingBox;
        [DataMember]
        [Description("Bounding box of the template image captured at design time")]        
        [Browsable(false)]
        public Rectangle BoundingBox
        {
            get => boundingBox;
            set => boundingBox = value;
        }

        #endregion General

        #region Retry Attempts

        [DataMember]
        [Description("Number of times to retry if control can't be located in first attempt")]
        [Display(Name = "Retry Attempts", Order = 10, GroupName = "Retry Settings")]
        public int RetryAttempts { get; set; } = 5;

        [DataMember]
        [Description("Interval in seconds between each retry attempt")]     
        [Display(Name = "Retry Interval", Order = 20, GroupName = "Retry Settings")]     
        public int RetryInterval { get; set; } = 2;

        #endregion Retry Attempts

        #region Clickable Point Offset

        Pivots pivotPoint = Pivots.Center;
        [DataMember]   
        [Display(Name = "Pivot Point", Order = 20, GroupName = "Clickable Point")]
        [ReadOnly(true)]
        public Pivots PivotPoint
        {
            get => pivotPoint;
            set
            {
                pivotPoint = value;
                OnPropertyChanged();
            }
        }

        protected double xOffset = 0.0f;
        [DataMember]
        [Description("X offset to be added to control top left coordinates if simulating mouse actions on this control")]
        [Display(Name = "X-Offset", Order = 20, GroupName = "Clickable Point")]
        [ReadOnly(true)]
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
        [Description("Y offset to be added to control top left coordinates if simulating mouse actions on this control")]
        [Display(Name = "Y-Offset", Order = 30, GroupName = "Clickable Point")]
        [ReadOnly(true)]
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

        #endregion Clickable Point Offset

        #region Search Strategy
               
        [DataMember]
        [Display(Name = "Search Scope", Order = 30, GroupName = "Search Strategy")]
        public virtual SearchScope SearchScope { get; set; } = SearchScope.Descendants;

        [DataMember]
        [Browsable(false)]
        [Display(Name = "Index", GroupName = "Search Mode", Order = 40)]
        [Description("Bind to current Iteration when used inside loop")]
        public int? Index { get; set; }

        #endregion Search Strategy

        #region Lookup Mode

        [DataMember]
        [Display(Name = "Look Up Type", GroupName = "Search Mode", Order = 10)]       
        [Description("Whether control should be looked relative to Application root or relative to a parent control")]
        public ControlType ControlType { get; set; } = ControlType.Default;     

        #endregion Lookup Mode

        [DataMember]
        [Browsable(false)]
        public IControlIdentity Next { get; set; }

        protected ControlIdentity()
        {
                     
        }

        public string GetControlName()
        {
            return this.Name;
        }

        public abstract object Clone();        
    }
}
