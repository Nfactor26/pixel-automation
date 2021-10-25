using System.Drawing;


namespace Pixel.Automation.Core.Interfaces
{
    public interface IScreenCapture
    {
        (short width, short height) GetScreenResolution();

        Bitmap CaptureDesktop();

        Bitmap CaptureArea(Rectangle rectangle);
    }
}
