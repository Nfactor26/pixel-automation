using Pixel.Automation.Core.Devices;

namespace Pixel.Automation.Native.Linux.Device;

public class SyntheticMouse : ISyntheticMouse
{
    public int MouseWheelClickSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool IsCriticalResource => true;

    public ISyntheticMouse ButtonDown(MouseButton mouseButton)
    {
        throw new NotImplementedException();
    }

    public ISyntheticMouse ButtonUp(MouseButton mouseButton)
    {
        throw new NotImplementedException();
    }

    public ISyntheticMouse Click(MouseButton mouseButton)
    {
        throw new NotImplementedException();
    }

    public ISyntheticMouse DoubleClick(MouseButton mouseButton)
    {
        throw new NotImplementedException();
    }

    public ScreenCoordinate GetCursorPosition()
    {
        throw new NotImplementedException();
    }

    public ISyntheticMouse HorizontalScroll(int scrollAmountInClicks)
    {
        throw new NotImplementedException();
    }

    public ISyntheticMouse MoveMouseBy(int pixelDeltaX, int pixelDeltaY, SmoothMode smoothMode)
    {
        throw new NotImplementedException();
    }

    public ISyntheticMouse MoveMouseTo(ScreenCoordinate screenCoordinate, SmoothMode smoothMode)
    {
        throw new NotImplementedException();
    }

    public ISyntheticMouse VerticalScroll(int scrollAmountInClicks)
    {
        throw new NotImplementedException();
    }
}
