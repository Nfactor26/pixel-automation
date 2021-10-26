using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IControlIdentity : ICloneable
    {      

        string ApplicationId
        {
            get;
            set;
        }

        string Name
        {
            get;
            set;
        }
        
        Rectangle BoundingBox
        {
            get;
            set;
        }

        /// <summary>
        /// Number of times to retry if control can't be located in first attempt
        /// </summary>
        int RetryAttempts
        {
            get;
            set;
        }

        /// <summary>
        /// Interval in seconds between each retry attempt
        /// </summary>
        int RetryInterval
        {
            get;
            set;
        }

        /// <summary>
        /// Reference point on control realtive to which mouse operations are performed.       
        /// </summary>
        Pivots PivotPoint
        {
            get;
            set;
        }

        /// <summary>
        /// Offset on x-axis from Pivot Point
        /// </summary>
        double XOffSet
        {
            get;
            set;
        }

        /// <summary>
        /// Offset on y-axis from Pivot Point
        /// </summary>
        double YOffSet
        {
            get;
            set;
        }

        /// <summary>
        /// Controls how the control lookup will be performed by <see cref="IControlLocator{T}"/>
        /// </summary>
        LookupType LookupType
        {
            get;
            set;
        }

        /// <summary>
        /// Get the Name of the control
        /// </summary>
        /// <returns></returns>
        string GetControlName();        

        /// <summary>
        /// For nested navigation
        /// </summary>
        IControlIdentity  Next
        {
            get; set;
        }
    }

    public interface IImageControlIdentity : IControlIdentity
    {
        void AddImage(ImageDescription imageDescription);

        void DeleteImage(ImageDescription imageDescription);

        IEnumerable<ImageDescription> GetImages();
    }
}
