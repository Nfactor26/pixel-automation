using Caliburn.Micro;
using Pixel.Automation.Core.Interfaces;
using System.Drawing;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IImageCaptureViewModel : IScreen
    {
        IControlIdentity GetCapturedImageControl();

        void Initialize(Bitmap screenShot);
    }
}
