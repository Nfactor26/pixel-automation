using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using System;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces
{
    /// <summary>
    /// Captures the details of a control that can be used to locate it at runtime
    /// </summary>
    public interface IControlIdentity : ICloneable
    {      
        /// <summary>
        /// Identifier of the owner application
        /// </summary>
        string ApplicationId
        {
            get;
            set;
        }

        /// <summary>
        /// Name of the control
        /// </summary>
        string Name
        {
            get;
            set;
        }
        
        /// <summary>
        /// Bounding box of the control captured at design time
        /// </summary>
        BoundingBox BoundingBox
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

    /// <summary>
    /// Captures the image that can be treated as a control for automation using image matching
    /// </summary>
    public interface IImageControlIdentity : IControlIdentity
    {
        /// <summary>
        /// Add a new image. There can be multiple image associate with a ImageControlIdentity with
        /// different configuration based on resolution and theme that will be matched at runtime to 
        /// pick the correct image for that environment.
        /// </summary>
        /// <param name="imageDescription"></param>
        void AddImage(ImageDescription imageDescription);

        /// <summary>
        /// Delete an image
        /// </summary>
        /// <param name="imageDescription"></param>
        void DeleteImage(ImageDescription imageDescription);

        /// <summary>
        /// Get all the available images
        /// </summary>
        /// <returns></returns>
        IEnumerable<ImageDescription> GetImages();
    }
}
