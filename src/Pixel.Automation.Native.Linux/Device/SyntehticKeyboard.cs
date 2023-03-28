using Pixel.Automation.Core.Devices;

namespace Pixel.Automation.Native.Linux.Device;

public class SyntehticKeyboard : ISyntheticKeyboard
{
    public bool IsCriticalResource => true;

    public IEnumerable<SyntheticKeyCode> GetSynthethicKeyCodes(string keyCode)
    {
        throw new NotImplementedException();
    }

    public bool IsModifierKey(SyntheticKeyCode keyCode)
    {
        throw new NotImplementedException();
    }

    public ISyntheticKeyboard KeyDown(SyntheticKeyCode keyCode)
    {
        throw new NotImplementedException();
    }

    public ISyntheticKeyboard KeyPress(SyntheticKeyCode keyCode)
    {
        throw new NotImplementedException();
    }

    public ISyntheticKeyboard KeyPress(params SyntheticKeyCode[] keyCodes)
    {
        throw new NotImplementedException();
    }

    public ISyntheticKeyboard KeyUp(SyntheticKeyCode keyCode)
    {
        throw new NotImplementedException();
    }

    public ISyntheticKeyboard ModifiedKeyStroke(IEnumerable<SyntheticKeyCode> modifierKeyCodes, IEnumerable<SyntheticKeyCode> keyCodes)
    {
        throw new NotImplementedException();
    }

    public ISyntheticKeyboard ModifiedKeyStroke(IEnumerable<SyntheticKeyCode> modifierKeyCodes, SyntheticKeyCode keyCode)
    {
        throw new NotImplementedException();
    }

    public ISyntheticKeyboard ModifiedKeyStroke(SyntheticKeyCode modifierKey, IEnumerable<SyntheticKeyCode> keyCodes)
    {
        throw new NotImplementedException();
    }

    public ISyntheticKeyboard ModifiedKeyStroke(SyntheticKeyCode modifierKeyCode, SyntheticKeyCode keyCode)
    {
        throw new NotImplementedException();
    }

    public ISyntheticKeyboard TypeCharacter(char character)
    {
        throw new NotImplementedException();
    }

    public ISyntheticKeyboard TypeText(string text)
    {
        throw new NotImplementedException();
    }
}
