using Dawn;
using InputSimulatorStandard;
using InputSimulatorStandard.Native;
using Pixel.Automation.Core.Devices;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pixel.Automation.Native.Windows.Device
{
    public class SyntheticKeyboard : ISyntheticKeyboard
    {
        private readonly object _lock = new object();
        private readonly IKeyboardSimulator keyboardSimulator;

        public SyntheticKeyboard()
        {
            this.keyboardSimulator = new KeyboardSimulator() ;
        }

        internal SyntheticKeyboard(IKeyboardSimulator keyboardSimulator)
        {
            this.keyboardSimulator = keyboardSimulator;
        }

        public bool IsCriticalResource => true;

        public ISyntheticKeyboard KeyDown(SyntheticKeyCode keyCode)
        {
            lock (_lock)
            {
                this.keyboardSimulator.KeyDown(ToVirtualKeyCode(keyCode));
                return this;
            }
        }

        public ISyntheticKeyboard KeyPress(SyntheticKeyCode keyCode)
        {
            lock (_lock)
            {
                this.keyboardSimulator.KeyPress(ToVirtualKeyCode(keyCode));
                return this;
            }
        }

        public ISyntheticKeyboard KeyPress(params SyntheticKeyCode[] keyCodes)
        {
            lock (_lock)
            {
                foreach (var keyCode in keyCodes)
                {
                    this.KeyPress(keyCode);
                }
                return this;
            }
        }

        public ISyntheticKeyboard KeyUp(SyntheticKeyCode keyCode)
        {
            lock (_lock)
            {
                this.keyboardSimulator.KeyUp(ToVirtualKeyCode(keyCode));
                return this;
            }
        }

        public ISyntheticKeyboard ModifiedKeyStroke(IEnumerable<SyntheticKeyCode> modifierKeyCodes, IEnumerable<SyntheticKeyCode> keyCodes)
        {
            lock (_lock)
            {
                this.keyboardSimulator.ModifiedKeyStroke(ToVirtualKeyCodes(modifierKeyCodes), ToVirtualKeyCodes(keyCodes));
                return this;
            }
        }

        public ISyntheticKeyboard ModifiedKeyStroke(IEnumerable<SyntheticKeyCode> modifierKeyCodes, SyntheticKeyCode keyCode)
        {
            lock (_lock)
            {
                this.keyboardSimulator.ModifiedKeyStroke(ToVirtualKeyCodes(modifierKeyCodes), ToVirtualKeyCode(keyCode));
                return this;
            }
        }

        public ISyntheticKeyboard ModifiedKeyStroke(SyntheticKeyCode modifierKey, IEnumerable<SyntheticKeyCode> keyCodes)
        {
            lock (_lock)
            {
                this.keyboardSimulator.ModifiedKeyStroke(ToVirtualKeyCode(modifierKey), ToVirtualKeyCodes(keyCodes));
                return this;
            }
        }

        public ISyntheticKeyboard ModifiedKeyStroke(SyntheticKeyCode modifierKeyCode, SyntheticKeyCode keyCode)
        {
            lock (_lock)
            {
                this.keyboardSimulator.ModifiedKeyStroke(ToVirtualKeyCode(modifierKeyCode), ToVirtualKeyCode(keyCode));
                return this;
            }
        }

        public ISyntheticKeyboard TypeCharacter(char character)
        {
            lock (_lock)
            {
                this.keyboardSimulator.TextEntry(character);
                return this;
            }
        }

        public ISyntheticKeyboard TypeText(string text)
        {
            lock (_lock)
            {
                this.keyboardSimulator.TextEntry(text);
                return this;
            }
        }
     
        Regex keysMatcher = new Regex(@"(([^+])+|([\+\s]){2})|(\+)", RegexOptions.Compiled);
        public IEnumerable<SyntheticKeyCode> GetSynthethicKeyCodes(string keyCode)
        {
            Guard.Argument<string>(keyCode).NotNull().NotEmpty().NotWhiteSpace();

            //Can't handle Num + since + is a seperator. Replace this with assumed alias.
            if(keyCode.Contains("Num +"))
            {
                keyCode = keyCode.Replace("Num +", "NumPadPlus");
            }

            var matches = keysMatcher.Matches(keyCode);
            if(matches.Count < 1)
            {
                throw new ArgumentException($"{keyCode} should be in the form key1 + key2 + key3....Failed to parse string representation to sequence of SytnetheticKeyCode");
            }
          
            var synthethicKeyCodes = new List<SyntheticKeyCode>();
            bool skip = false;
            foreach (Match match in matches)
            {
                if(!skip)
                {
                    synthethicKeyCodes.Add(ToSyntheticKeyCode(match.Value.Trim()));
                }
                skip = !skip;
            }
            return synthethicKeyCodes;

           SyntheticKeyCode ToSyntheticKeyCode(string key)
           {
                switch (key)
                {
                    case "Alt":
                        return SyntheticKeyCode.MENU;
                    case "Ctrl":
                        return SyntheticKeyCode.CONTROL;
                    case "Shift":
                        return SyntheticKeyCode.SHIFT;
                    case "Windows":
                        return SyntheticKeyCode.LWIN;

                    case "A":
                        return SyntheticKeyCode.VK_A;
                    case "B":
                        return SyntheticKeyCode.VK_B;
                    case "C":
                        return SyntheticKeyCode.VK_C;
                    case "D":
                        return SyntheticKeyCode.VK_D;
                    case "E":
                        return SyntheticKeyCode.VK_E;
                    case "F":
                        return SyntheticKeyCode.VK_F;
                    case "G":
                        return SyntheticKeyCode.VK_G;
                    case "H":
                        return SyntheticKeyCode.VK_H;
                    case "I":
                        return SyntheticKeyCode.VK_I;
                    case "J":
                        return SyntheticKeyCode.VK_J;
                    case "K":
                        return SyntheticKeyCode.VK_K;
                    case "L":
                        return SyntheticKeyCode.VK_L;
                    case "M":
                        return SyntheticKeyCode.VK_M;
                    case "N":
                        return SyntheticKeyCode.VK_N;
                    case "O":
                        return SyntheticKeyCode.VK_O;
                    case "P":
                        return SyntheticKeyCode.VK_P;
                    case "Q":
                        return SyntheticKeyCode.VK_Q;
                    case "R":
                        return SyntheticKeyCode.VK_R;
                    case "S":
                        return SyntheticKeyCode.VK_S;
                    case "T":
                        return SyntheticKeyCode.VK_T;
                    case "U":
                        return SyntheticKeyCode.VK_U;
                    case "V":
                        return SyntheticKeyCode.VK_V;
                    case "W":
                        return SyntheticKeyCode.VK_W;
                    case "X":
                        return SyntheticKeyCode.VK_X;
                    case "Y":
                        return SyntheticKeyCode.VK_Y;
                    case "Z":
                        return SyntheticKeyCode.VK_Z;

                    case "Num 0":
                    case "0":
                        return SyntheticKeyCode.VK_0;
                    case "Num 1":
                    case "1":
                        return SyntheticKeyCode.VK_1;
                    case "Num 2":
                    case "2":
                        return SyntheticKeyCode.VK_2;
                    case "Num 3":
                    case "3":
                        return SyntheticKeyCode.VK_3;
                    case "Num 4":
                    case "4":
                        return SyntheticKeyCode.VK_4;
                    case "Num 5":
                    case "5":
                        return SyntheticKeyCode.VK_5;
                    case "Num 6":
                    case "6":
                        return SyntheticKeyCode.VK_6;
                    case "Num 7":
                    case "7":
                        return SyntheticKeyCode.VK_7;
                    case "Num 8":
                    case "8":
                        return SyntheticKeyCode.VK_8;
                    case "Num 9":
                    case "9":
                        return SyntheticKeyCode.VK_9;

                    case "+":
                    case "Num +":
                    case "NumPadPlus":
                        return SyntheticKeyCode.ADD;
                    case "-":
                    case "Num -":
                        return SyntheticKeyCode.SUBTRACT;
                    case "*":
                    case "Num *":
                        return SyntheticKeyCode.MULTIPLY;
                    case "/":
                    case "Num /":
                        return SyntheticKeyCode.DIVIDE;

                    case "Enter":
                        return SyntheticKeyCode.RETURN;
                    case "Backspace":
                        return SyntheticKeyCode.BACK;
                    case "Esc":
                        return SyntheticKeyCode.ESCAPE;
                    case "Caps Lock":
                        return SyntheticKeyCode.CAPITAL;
                    case "Scroll Lock":
                        return SyntheticKeyCode.SCROLL;
                    case "Num Lock":
                        return SyntheticKeyCode.NUMLOCK;
                    case "Num Delete":
                        return SyntheticKeyCode.DECIMAL;

                    case "Page Up":
                        return SyntheticKeyCode.PRIOR;
                    case "Page Down":
                        return SyntheticKeyCode.NEXT;
                    case "Insert":
                        return SyntheticKeyCode.INSERT;
                    case "Delete":
                        return SyntheticKeyCode.DELETE;
                    case "Home":
                        return SyntheticKeyCode.HOME;
                    case "End":
                        return SyntheticKeyCode.END;


                    case "VolumeDown":
                        return SyntheticKeyCode.VOLUME_DOWN;
                    case "VolumeUp":
                        return SyntheticKeyCode.VOLUME_UP;

                    default:
                        if (Enum.TryParse<SyntheticKeyCode>(key.ToUpper(), out SyntheticKeyCode sytntheticKeyCode))
                        {
                            return sytntheticKeyCode;
                        }
                        throw new ArgumentException($"{key} could not be parsed to type : {typeof(SyntheticKeyCode)}");
                }
            }
           
        }

        public bool IsModifierKey(SyntheticKeyCode keyCode)
        {
            switch (keyCode)
            {
                case SyntheticKeyCode.LCONTROL:
                case SyntheticKeyCode.RCONTROL:
                case SyntheticKeyCode.CONTROL:
                case SyntheticKeyCode.LSHIFT:
                case SyntheticKeyCode.RSHIFT:
                case SyntheticKeyCode.SHIFT:
                case SyntheticKeyCode.LWIN:
                case SyntheticKeyCode.RWIN:
                case SyntheticKeyCode.LMENU:
                case SyntheticKeyCode.RMENU:
                case SyntheticKeyCode.MENU:
                    return true;
                default:
                    return false;
            }
        }

        VirtualKeyCode ToVirtualKeyCode(SyntheticKeyCode syntheticKeyCode)
        {
            return (VirtualKeyCode)((int)syntheticKeyCode);
        }

        IEnumerable<VirtualKeyCode> ToVirtualKeyCodes(IEnumerable<SyntheticKeyCode> syntheticKeyCodes)
        {
            foreach (var keyCode in syntheticKeyCodes)
            {
                yield return (VirtualKeyCode)((int)keyCode);
            }
        }

    }
}
