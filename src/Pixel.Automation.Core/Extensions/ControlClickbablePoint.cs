using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using System.Drawing;

namespace Pixel.Automation.Core.Extensions
{
    public static class ControlHelpers
    {
        /// <summary>
        /// Calculate the (x,y) coordinates to perform mouse operation. Runtime bounding box of control and configured pivot and offsets
        /// of control are used as input to calculate this point
        /// </summary>
        /// <param name="controlIdentity">Details of the control</param>
        /// <param name="boundingBox">Runtime bounding box of the control on screen</param>
        /// <param name="x">Calculated x-coord to perform mouse operations at</param>
        /// <param name="y">Calculated y-coord to perform mouse operations at</param>
        public static void GetClickablePoint(this IControlIdentity controlIdentity, Rectangle boundingBox,out double x, out double y)
        {
            //target control is the leaf node in controlIdentity and contains the actual configuration for pivot and offset.
            var targetControl = controlIdentity;
            while (targetControl.Next != null)
            {
                targetControl = targetControl.Next;
            }

            switch (targetControl.PivotPoint)
            {
                case Pivots.Center:
                    x = boundingBox.X + boundingBox.Width / 2 + targetControl.XOffSet;
                    y = boundingBox.Y + boundingBox.Height / 2 + targetControl.YOffSet;
                    break;
                case Pivots.TopLeft:
                    x = boundingBox.X + targetControl.XOffSet;
                    y = boundingBox.Y + targetControl.YOffSet;
                    break;
                case Pivots.TopRight:
                    x = boundingBox.X + boundingBox.Width + targetControl.XOffSet;
                    y = boundingBox.Y + targetControl.YOffSet;
                    break;
                case Pivots.BottomLeft:
                    x = boundingBox.X + targetControl.XOffSet;
                    y = boundingBox.Y + boundingBox.Height + targetControl.YOffSet;
                    break;
                case Pivots.BottomRight:
                    x = boundingBox.X + boundingBox.Width + targetControl.XOffSet;
                    y = boundingBox.Y + boundingBox.Height + targetControl.YOffSet;
                    break;
                default:
                    x = 0.0f;
                    y = 0.0f;
                    break;
            }
        }
    }
}
