using System.Drawing;


namespace Pixel.Automation.Core.Interfaces
{
    /// <summary>
    /// ISreenCapture
    /// </summary>
    public interface IScreenCapture
    {
        /// <summary>
        /// Get current resolution of the screen
        /// </summary>
        /// <returns></returns>
        (short width, short height) GetScreenResolution();

        /// <summary>
        /// Capture the screen shot of entire desktop
        /// </summary>
        /// <returns></returns>
        Bitmap CaptureDesktop();

        /// <summary>
        /// Capture the specified area on the desktop
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        Bitmap CaptureArea(Rectangle rectangle);
    }
}
