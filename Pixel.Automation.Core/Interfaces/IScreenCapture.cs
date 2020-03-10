using System.Drawing;


namespace Pixel.Automation.Core.Interfaces
{
    public interface IScreenCapture
    {
        Bitmap CaptureDesktop();

        Bitmap CaptureArea(Rectangle rectangle);
    }
}
