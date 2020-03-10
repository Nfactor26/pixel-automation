using System.Collections.Generic;

namespace Pixel.Automation.Core.Devices
{
    public interface ISyntheticKeyboard : IDevice
    {
        /// <summary>
        /// Simulates the key down gesture for the specified key.
        /// </summary>
        /// <param name="keyCode">The <see cref="SyntheticKeyCode"/> for the key.</param>
        ISyntheticKeyboard KeyDown(SyntheticKeyCode keyCode);

        /// <summary>
        /// Simulates the key press gesture for the specified key.
        /// </summary>
        /// <param name="keyCode">The <see cref="SyntheticKeyCode"/> for the key.</param>
        ISyntheticKeyboard KeyPress(SyntheticKeyCode keyCode);

        /// <summary>
        /// Simulates a key press for each of the specified key codes in the order they are specified.
        /// </summary>
        /// <param name="keyCodes"></param>
        ISyntheticKeyboard KeyPress(params SyntheticKeyCode[] keyCodes);

        /// <summary>
        /// Simulates the key up gesture for the specified key.
        /// </summary>
        /// <param name="keyCode">The <see cref="SyntheticKeyCode"/> for the key.</param>
        ISyntheticKeyboard KeyUp(SyntheticKeyCode keyCode);

        /// <summary>
        /// Simulates a modified keystroke where there are multiple modifiers and multiple keys like CTRL-ALT-K-C where CTRL and ALT are the modifierKeys and K and C are the keys.
        /// The flow is Modifiers KeyDown in order, Keys Press in order, Modifiers KeyUp in reverse order.
        /// </summary>
        /// <param name="modifierKeyCodes">The list of <see cref="VirtualKeyCode"/>s for the modifier keys.</param>
        /// <param name="keyCodes">The list of <see cref="VirtualKeyCode"/>s for the keys to simulate.</param>
        ISyntheticKeyboard ModifiedKeyStroke(IEnumerable<SyntheticKeyCode> modifierKeyCodes, IEnumerable<SyntheticKeyCode> keyCodes);

        /// <summary>
        /// Simulates a modified keystroke where there are multiple modifiers and one key like CTRL-ALT-C where CTRL and ALT are the modifierKeys and C is the key.
        /// The flow is Modifiers KeyDown in order, Key Press, Modifiers KeyUp in reverse order.
        /// </summary>
        /// <param name="modifierKeyCodes">The list of <see cref="VirtualKeyCode"/>s for the modifier keys.</param>
        /// <param name="keyCode">The <see cref="VirtualKeyCode"/> for the key.</param>
        ISyntheticKeyboard ModifiedKeyStroke(IEnumerable<SyntheticKeyCode> modifierKeyCodes, SyntheticKeyCode keyCode);

        /// <summary>
        /// Simulates a modified keystroke where there is one modifier and multiple keys like CTRL-K-C where CTRL is the modifierKey and K and C are the keys.
        /// The flow is Modifier KeyDown, Keys Press in order, Modifier KeyUp.
        /// </summary>
        /// <param name="modifierKey">The <see cref="SyntheticKeyCode"/> for the modifier key.</param>
        /// <param name="keyCodes">The list of <see cref="SyntheticKeyCode"/>s for the keys to simulate.</param>
        ISyntheticKeyboard ModifiedKeyStroke(SyntheticKeyCode modifierKey, IEnumerable<SyntheticKeyCode> keyCodes);

        /// <summary>
        /// Simulates a simple modified keystroke like CTRL-C where CTRL is the modifierKey and C is the key.
        /// The flow is Modifier KeyDown, Key Press, Modifier KeyUp.
        /// </summary>
        /// <param name="modifierKeyCode">The <see cref="VirtualKeyCode"/> for the  modifier key.</param>
        /// <param name="keyCode">The <see cref="VirtualKeyCode"/> for the key.</param>
        ISyntheticKeyboard ModifiedKeyStroke(SyntheticKeyCode modifierKeyCode, SyntheticKeyCode keyCode);

        /// <summary>
        /// Simulates uninterrupted text entry via the keyboard.
        /// </summary>
        /// <param name="text">The text to be simulated.</param>
        ISyntheticKeyboard TypeText(string text);

        /// <summary>
        /// Simulates a single character text entry via the keyboard.
        /// </summary>
        /// <param name="character">The unicode character to be simulated.</param>
        ISyntheticKeyboard TypeCharacter(char character);

        /// <summary>
        /// Parse a input string to sequence of SyntheticKeyCode
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        IEnumerable<SyntheticKeyCode> GetSynthethicKeyCodes(string keyCode);

        /// <summary>
        /// Indicates whether the keycode represents a modifier key e.g. ctrl , alt , etc.
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        bool IsModifierKey(SyntheticKeyCode keyCode);


    }
}
