using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Controls
{
    /// <summary>
    /// ControlIdentity is used to store the common details of a control irrespective of ui framework being automated.
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class ControlIdentity : NotifyPropertyChanged, IControlIdentity, ICloneable
    {
        #region  General 

        /// <summary>
        /// Identifier of the owner application
        /// </summary>
        [DataMember(Order = 10)]
        [Browsable(false)]
        public string ApplicationId { get; set; }

        /// <summary>
        /// Name of the control
        /// </summary>
        [DataMember(Order = 20)]
        public string Name { get; set; }

        protected BoundingBox boundingBox;
        /// <summary>
        /// <see cref="BoundingBox"/> of the control captured during scraping the control
        /// </summary>
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

        /// <summary>
        /// Number of RetryAttempts while looking up this control at runtime
        /// </summary>
        [DataMember(Order = 50)]
        [Description("Number of times to retry if control can't be located in first attempt")]
        [Display(Name = "Retry Attempts", Order = 10, GroupName = "Retry Settings")]
        public int RetryAttempts { get; set; } = 5;

        /// <summary>
        /// Time interval in seconds between each retry attempt
        /// </summary>
        [DataMember(Order = 60)]
        [Description("Interval in seconds between each retry attempt")]
        [Display(Name = "Retry Interval", Order = 20, GroupName = "Retry Settings")]
        public int RetryInterval { get; set; } = 2;

        #endregion Retry Attempts

        #region Clickable Point Offset

        /// <summary>
        /// <see cref="Pivots"/> point  for XOffset and YOffset
        /// </summary>
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
        /// <summary>
        /// XOffset from the PivotPoint
        /// </summary>
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

        /// <summary>
        /// YOffset from the PivotPoint
        /// </summary>
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

        /// <summary>
        /// <see cref="LookupType"/> for the search strategy
        /// </summary>
        [DataMember(Order = 100)]
        [Display(Name = "Look Up Type", GroupName = "Search Strategy", Order = 10)]
        [Description("Whether control should be looked relative to Application root or relative to a parent control")]
        public LookupType LookupType { get; set; } = LookupType.Default;

        /// <summary>
        /// <see cref="SearchScope"/> for the search strategy
        /// </summary>
        [DataMember(Order = 110)]
        [Display(Name = "Search Scope", Order = 20, GroupName = "Search Strategy")]
        public virtual SearchScope SearchScope { get; set; } = SearchScope.Descendants;

        /// <summary>
        /// Control Identifier along with configured search strategy can end up matching multiple controls.
        /// Index can be used to pick a control at given index out of multiple matches.
        /// </summary>
        [DataMember(Order = 120)]
        [Browsable(false)]
        [Display(Name = "Index", GroupName = "Search Strategy", Order = 30)]
        [Description("Index of the control to pick when there are multiple matches")]
        public int Index { get; set; } = 1;

        #endregion Search Strategy

        /// <summary>
        /// Control can be looked up in to multiple steps from a top level control traversing top-down through the control tree.
        /// Next holds the ControlIdentity for the next control to be located in control tree.
        /// </summary>
        [DataMember(Order = 1000)]
        [Browsable(false)]
        public IControlIdentity Next { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        protected ControlIdentity()
        {

        }

        /// <summary>
        /// Get the name of the control
        /// </summary>
        /// <returns></returns>
        public string GetControlName()
        {
            return this.Name;
        }

        /// <inheritdoc/>
        public abstract object Clone();
    }
}
