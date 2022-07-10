using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Controls
{
    [DataContract]
    [Serializable]
    public abstract class ControlIdentity : NotifyPropertyChanged, IControlIdentity , ICloneable
    {
        #region  General 

        [DataMember(Order = 10)]
        [Browsable(false)]
        public string ApplicationId { get; set; }

        [DataMember(Order = 20)]
        public string Name { get; set; }      

        protected BoundingBox boundingBox;
        [DataMember(Order = 40)]
        [Description("Bounding box of the template image captured at design time")]        
        [Browsable(false)]
        public BoundingBox BoundingBox
        {
            get => boundingBox;
            set => boundingBox = value;
        }

        #endregion General

        #region Retry Attempts

        [DataMember(Order = 50)]
        [Description("Number of times to retry if control can't be located in first attempt")]
        [Display(Name = "Retry Attempts", Order = 10, GroupName = "Retry Settings")]
        public int RetryAttempts { get; set; } = 5;

        [DataMember(Order = 60)]
        [Description("Interval in seconds between each retry attempt")]     
        [Display(Name = "Retry Interval", Order = 20, GroupName = "Retry Settings")]     
        public int RetryInterval { get; set; } = 2;

        #endregion Retry Attempts

        #region Clickable Point Offset

        Pivots pivotPoint = Pivots.Center;
        [DataMember(Order = 70)]
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
        [DataMember(Order = 80)]
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
        [DataMember(Order = 90)]
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

        [DataMember(Order = 100)]
        [Display(Name = "Look Up Type", GroupName = "Search Strategy", Order = 10)]
        [Description("Whether control should be looked relative to Application root or relative to a parent control")]
        public LookupType LookupType { get; set; } = LookupType.Default;


        [DataMember(Order = 110)]
        [Display(Name = "Search Scope", Order = 20, GroupName = "Search Strategy")]
        public virtual SearchScope SearchScope { get; set; } = SearchScope.Descendants;

        [DataMember(Order = 120)]
        [Browsable(false)]
        [Display(Name = "Index", GroupName = "Search Strategy", Order = 30)]
        [Description("Bind to current Iteration when used inside loop")]
        public int Index { get; set; } = 1;

        #endregion Search Strategy

        [DataMember(Order = 1000)]
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
