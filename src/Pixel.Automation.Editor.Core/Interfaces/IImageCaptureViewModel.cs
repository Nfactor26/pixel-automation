using Caliburn.Micro;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IImageCaptureViewModel : IScreen
    {
        IControlIdentity GetCapturedImageControl();

        void Initialize(Bitmap screenShot);
    }
}
