using System.Drawing;

namespace Pixel.Automation.Core.Devices
{
    public interface ISyntheticMouse : IDevice
    {
        /// <summary>
        /// Gets or sets the amount of mouse wheel scrolling per click. The default value for this property is 120 and different values may cause some applications to interpret the scrolling differently than expected.
        /// </summary>
        int MouseWheelClickSize { get; set; }

        /// <summary>
        /// Hold down specified mouse button e.g. hold down left to start dragging
        /// </summary>
        /// <param name="mouseButton"></param>
        /// <returns></returns>
        ISyntheticMouse ButtonDown(MouseButton mouseButton);

        /// <summary>
        /// Release a specified mouse button which was earlier hold down 
        /// </summary>
        /// <param name="mouseButton"></param>
        /// <returns></returns>
        ISyntheticMouse ButtonUp(MouseButton mouseButton);

        /// <summary>
        /// Click specified mouse button
        /// </summary>
        /// <param name="mouseButton"></param>
        /// <returns></returns>
        ISyntheticMouse Click(MouseButton mouseButton);

        /// <summary>
        /// Double Click specified mouse button
        /// </summary>
        /// <param name="mouseButton"></param>
        /// <returns></returns>
        ISyntheticMouse DoubleClick(MouseButton mouseButton);


        /// <summary>
        /// Simulates mouse movement by the specified distance measured as a delta from the current mouse location in pixels.
        /// </summary>
        /// <param name="pixelDeltaX">The distance in pixels to move the mouse horizontally.</param>
        /// <param name="pixelDeltaY">The distance in pixels to move the mouse vertically.</param>
        ISyntheticMouse MoveMouseBy(int pixelDeltaX, int pixelDeltaY, SmoothMode smoothMode);

        /// <summary>
        /// Simulates mouse movement to the specified location on the primary display device.
        /// </summary>
        /// <param name="absoluteX">The destination's absolute X-coordinate on the primary display device where 0 is the extreme left hand side of the display device and 65535 is the extreme right hand side of the display device.</param>
        /// <param name="absoluteY">The destination's absolute Y-coordinate on the primary display device where 0 is the top of the display device and 65535 is the bottom of the display device.</param>
        ISyntheticMouse MoveMouseTo(ScreenCoordinate screenCoordinate, SmoothMode smoothMode);

        /// <summary>
        /// Simulates mouse vertical wheel scroll gesture.
        /// </summary>
        /// <param name="scrollAmountInClicks">The amount to scroll in clicks. A positive value indicates that the wheel was rotated forward, away from the user; a negative value indicates that the wheel was rotated backward, toward the user.</param>
        ISyntheticMouse VerticalScroll(int scrollAmountInClicks);

        /// <summary>
        /// Simulates a mouse horizontal wheel scroll gesture. Supported by Windows Vista and later.
        /// </summary>
        /// <param name="scrollAmountInClicks">The amount to scroll in clicks. A positive value indicates that the wheel was rotated to the right; a negative value indicates that the wheel was rotated to the left.</param>
        ISyntheticMouse HorizontalScroll(int scrollAmountInClicks);

        /// <summary>
        /// Retrieve the current location of cursor
        /// </summary>
        /// <returns></returns>
        Point GetCursorPosition();

    }
}
