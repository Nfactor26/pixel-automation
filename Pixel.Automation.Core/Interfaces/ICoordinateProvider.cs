using System.Drawing;

namespace Pixel.Automation.Core.Interfaces
{
    /// <summary>
    /// Provides the position details of the control as displayed on screen
    /// </summary>
    public interface ICoordinateProvider
    {
        bool CanProcessControlOfType(IControlIdentity controlIdentity);

        /// <summary>
        /// Get clickable point inside the control
        /// </summary>
        /// <param name="targetControl"><see cref="IControlIdentity"/> for which clickable point is required</param>
        /// <param name="x">x coordinate of the clickable point</param>
        /// <param name="y">y coordinate of the clickable point</param>
        void GetClickablePoint(IControlIdentity targetControl,out double x, out double y);

        /// <summary>
        /// Get bounding box of the control on screen
        /// </summary>
        /// <param name="targetControl"><see cref="IControlIdentity"/> whose bounding box is required</param>
        /// <param name="screenBounds"><see cref="Rectangle"/> as the bounding box</param>
        void GetScreenBounds(IControlIdentity targetControl, out Rectangle screenBounds);
    }
}
