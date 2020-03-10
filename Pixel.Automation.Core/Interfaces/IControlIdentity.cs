using Pixel.Automation.Core.Enums;
using System;
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

        string ControlImage
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
        ControlType ControlType
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
        /// Get the Owner application name
        /// </summary>
        /// <returns></returns>
        //string GetOwnerAppName();

        /// <summary>
        /// Get the IControlLocator for IControlIdentity
        /// </summary>
        /// <returns></returns>
        //IControlLocator<object> GetControlLocator();

        /// <summary>
        /// Get the ICoordinateProvider implementatoin for IControlIdentity
        /// </summary>
        /// <returns></returns>
        //ICoordinateProvider GetCoordinateProvider();

        /// <summary>
        /// For nested navigation
        /// </summary>
        IControlIdentity  Next
        {
            get; set;
        }
    }
}
