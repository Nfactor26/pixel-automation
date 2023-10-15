using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using System;

namespace Pixel.Automation.Native.Linux;

public class ScreenCapture : IScreenCapture
{
    public byte[] CaptureArea(BoundingBox rectangle)
    {
        throw new NotImplementedException();
    }

    public byte[] CaptureDesktop()
    {
        throw new NotImplementedException();
    }

    public (short width, short height) GetScreenResolution()
    {
        throw new NotImplementedException();
    }
}
