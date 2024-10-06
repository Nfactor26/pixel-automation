using InputSimulatorStandard;
using InputSimulatorStandard.Native;
using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Native.Windows.Device;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Automation.Nativew.Windows.Tests.Device
{
    class SyntheticKeyboardFixture
    {

        private readonly SyntheticKeyboard syntheticKeyboard;
        private readonly IKeyboardSimulator keyboardSimulator;
        
        public SyntheticKeyboardFixture()
        {
            this.keyboardSimulator = Substitute.For<IKeyboardSimulator>();
            this.syntheticKeyboard = new SyntheticKeyboard(this.keyboardSimulator);
        }

        [TearDown]
        public void TearDown()
        {
            this.keyboardSimulator.ClearReceivedCalls();
        }

        [Test]
        public void ValidateThatSyntheticKeyboardCanDoKeyDown()
        {
            this.syntheticKeyboard.KeyDown(SyntheticKeyCode.VK_C);
            this.keyboardSimulator.Received(1).KeyDown(Arg.Is<VirtualKeyCode>(VirtualKeyCode.VK_C));
        }

        [Test]
        public void ValidateThatSyntheticKeyboardCanDoKeyPress()
        {
            this.syntheticKeyboard.KeyPress(SyntheticKeyCode.VK_C);
            this.keyboardSimulator.Received(1).KeyPress(Arg.Is<VirtualKeyCode>(VirtualKeyCode.VK_C));
        }

        [Test]
        public void ValidateThatSyntheticKeyboardCanDoMultipleKeyPress()
        {
            this.syntheticKeyboard.KeyPress(new[] { SyntheticKeyCode.LCONTROL, SyntheticKeyCode.VK_C  });
            this.keyboardSimulator.Received(1).KeyPress(Arg.Is<VirtualKeyCode>(VirtualKeyCode.LCONTROL));        
            this.keyboardSimulator.Received(1).KeyPress(Arg.Is<VirtualKeyCode>(VirtualKeyCode.VK_C));
        }

        [Test]
        public void ValidateThatSyntheticKeyboardCanDoKeyUp()
        {
            this.syntheticKeyboard.KeyUp(SyntheticKeyCode.VK_C);
            this.keyboardSimulator.Received(1).KeyUp(Arg.Is<VirtualKeyCode>(VirtualKeyCode.VK_C));
        }

        [Test]
        public void ValidateThatSyntheticKeyboardCanDoHotKeys()
        {
            this.syntheticKeyboard.ModifiedKeyStroke(SyntheticKeyCode.LCONTROL, SyntheticKeyCode.VK_C);
            this.keyboardSimulator.Received(1).ModifiedKeyStroke(Arg.Is<VirtualKeyCode>(VirtualKeyCode.LCONTROL), Arg.Is<VirtualKeyCode>(VirtualKeyCode.VK_C));
            this.keyboardSimulator.ClearReceivedCalls();

            this.syntheticKeyboard.ModifiedKeyStroke(SyntheticKeyCode.LCONTROL, new[] { SyntheticKeyCode.VK_A, SyntheticKeyCode.VK_C });
            this.keyboardSimulator.Received(1).ModifiedKeyStroke(Arg.Is<VirtualKeyCode>(VirtualKeyCode.LCONTROL), Arg.Any<IEnumerable<VirtualKeyCode>>());
            this.keyboardSimulator.ClearReceivedCalls();

            this.syntheticKeyboard.ModifiedKeyStroke(new[] { SyntheticKeyCode.LCONTROL, SyntheticKeyCode.LSHIFT }, SyntheticKeyCode.VK_A);
            this.keyboardSimulator.Received(1).ModifiedKeyStroke(Arg.Any<IEnumerable<VirtualKeyCode>>(), Arg.Is<VirtualKeyCode>(VirtualKeyCode.VK_A));
            this.keyboardSimulator.ClearReceivedCalls();

            this.syntheticKeyboard.ModifiedKeyStroke( new[] { SyntheticKeyCode.LCONTROL} , new[] { SyntheticKeyCode.VK_C });
            this.keyboardSimulator.Received(1).ModifiedKeyStroke(Arg.Any<IEnumerable<VirtualKeyCode>>(), Arg.Any<IEnumerable<VirtualKeyCode>>());
            this.keyboardSimulator.ClearReceivedCalls();
        }

        [Test]
        public void ValidateThatSyntheticKeyboardCanTypeCharacter()
        {
            this.syntheticKeyboard.TypeCharacter('h');
            this.keyboardSimulator.Received(1).TextEntry(Arg.Is<char>('h'));
        }


        [Test]
        public void ValidateThatSyntheticKeyboardCanTypeText()
        {
            this.syntheticKeyboard.TypeText("Hello");
            this.keyboardSimulator.Received(1).TextEntry(Arg.Is<string>("Hello"));
        }

        [TestCase(SyntheticKeyCode.LCONTROL, true)]
        [TestCase(SyntheticKeyCode.RCONTROL, true)]
        [TestCase(SyntheticKeyCode.LSHIFT, true)]
        [TestCase(SyntheticKeyCode.RSHIFT, true)]
        [TestCase(SyntheticKeyCode.SHIFT, true)]
        [TestCase(SyntheticKeyCode.LWIN, true)]
        [TestCase(SyntheticKeyCode.RWIN, true)]
        [TestCase(SyntheticKeyCode.LMENU, true)]
        [TestCase(SyntheticKeyCode.RMENU, true)]
        [TestCase(SyntheticKeyCode.MENU, true)]
        [TestCase(SyntheticKeyCode.VK_C, false)]
        public void ValidateThatSyntheticKeyboardCanRecognizeModifierKeys(SyntheticKeyCode keyCode, bool expectedResult)
        {
            bool isModifierKey = this.syntheticKeyboard.IsModifierKey(keyCode);
            Assert.That(isModifierKey, Is.EqualTo(expectedResult));
        }


        [TestCase("Alt", SyntheticKeyCode.MENU)]
        [TestCase("Ctrl", SyntheticKeyCode.CONTROL)]
        [TestCase("Shift", SyntheticKeyCode.SHIFT)]
        [TestCase("Windows", SyntheticKeyCode.LWIN)]
        [TestCase("A", SyntheticKeyCode.VK_A)]
        [TestCase("B", SyntheticKeyCode.VK_B)]
        [TestCase("C", SyntheticKeyCode.VK_C)]
        [TestCase("D", SyntheticKeyCode.VK_D)]
        [TestCase("E", SyntheticKeyCode.VK_E)]
        [TestCase("F", SyntheticKeyCode.VK_F)]
        [TestCase("G", SyntheticKeyCode.VK_G)]
        [TestCase("H", SyntheticKeyCode.VK_H)]
        [TestCase("I", SyntheticKeyCode.VK_I)]
        [TestCase("J", SyntheticKeyCode.VK_J)]
        [TestCase("K", SyntheticKeyCode.VK_K)]
        [TestCase("M", SyntheticKeyCode.VK_M)]
        [TestCase("N", SyntheticKeyCode.VK_N)]
        [TestCase("O", SyntheticKeyCode.VK_O)]
        [TestCase("P", SyntheticKeyCode.VK_P)]
        [TestCase("Q", SyntheticKeyCode.VK_Q)]
        [TestCase("R", SyntheticKeyCode.VK_R)]
        [TestCase("O", SyntheticKeyCode.VK_O)]
        [TestCase("P", SyntheticKeyCode.VK_P)]
        [TestCase("Q", SyntheticKeyCode.VK_Q)]
        [TestCase("R", SyntheticKeyCode.VK_R)]
        [TestCase("S", SyntheticKeyCode.VK_S)]
        [TestCase("T", SyntheticKeyCode.VK_T)]
        [TestCase("U", SyntheticKeyCode.VK_U)]
        [TestCase("V", SyntheticKeyCode.VK_V)]
        [TestCase("W", SyntheticKeyCode.VK_W)]
        [TestCase("X", SyntheticKeyCode.VK_X)]
        [TestCase("Y", SyntheticKeyCode.VK_Y)]
        [TestCase("Z", SyntheticKeyCode.VK_Z)]
        [TestCase("0", SyntheticKeyCode.VK_0)]
        [TestCase("Num 0", SyntheticKeyCode.VK_0)]
        [TestCase("1", SyntheticKeyCode.VK_1)]
        [TestCase("Num 1", SyntheticKeyCode.VK_1)]
        [TestCase("2", SyntheticKeyCode.VK_2)]
        [TestCase("Num 2", SyntheticKeyCode.VK_2)]
        [TestCase("3", SyntheticKeyCode.VK_3)]
        [TestCase("Num 3", SyntheticKeyCode.VK_3)]
        [TestCase("4", SyntheticKeyCode.VK_4)]
        [TestCase("Num 4", SyntheticKeyCode.VK_4)]
        [TestCase("5", SyntheticKeyCode.VK_5)]
        [TestCase("Num 5", SyntheticKeyCode.VK_5)]
        [TestCase("6", SyntheticKeyCode.VK_6)]
        [TestCase("Num 6", SyntheticKeyCode.VK_6)]
        [TestCase("7", SyntheticKeyCode.VK_7)]
        [TestCase("Num 7", SyntheticKeyCode.VK_7)]
        [TestCase("8", SyntheticKeyCode.VK_8)]
        [TestCase("Num 8", SyntheticKeyCode.VK_8)]
        [TestCase("9", SyntheticKeyCode.VK_9)]
        [TestCase("Num 9", SyntheticKeyCode.VK_9)]
        [TestCase("+", SyntheticKeyCode.ADD)]
        [TestCase("Num +", SyntheticKeyCode.ADD)]
        [TestCase("-", SyntheticKeyCode.SUBTRACT)]
        [TestCase("Num -", SyntheticKeyCode.SUBTRACT)]
        [TestCase("*", SyntheticKeyCode.MULTIPLY)]
        [TestCase("Num *", SyntheticKeyCode.MULTIPLY)]
        [TestCase("/", SyntheticKeyCode.DIVIDE)]
        [TestCase("Num /", SyntheticKeyCode.DIVIDE)]
        [TestCase("Enter", SyntheticKeyCode.RETURN)]
        [TestCase("Backspace", SyntheticKeyCode.BACK)]
        [TestCase("Esc", SyntheticKeyCode.ESCAPE)]
        [TestCase("Caps Lock", SyntheticKeyCode.CAPITAL)]
        [TestCase("Scroll Lock", SyntheticKeyCode.SCROLL)]
        [TestCase("Num Lock", SyntheticKeyCode.NUMLOCK)]
        [TestCase("Num Delete", SyntheticKeyCode.DECIMAL)]
        [TestCase("Page Up", SyntheticKeyCode.PRIOR)]
        [TestCase("Page Down", SyntheticKeyCode.NEXT)]
        [TestCase("Insert", SyntheticKeyCode.INSERT)]
        [TestCase("Delete", SyntheticKeyCode.DELETE)]
        [TestCase("Home", SyntheticKeyCode.HOME)]
        [TestCase("End", SyntheticKeyCode.END)]
        [TestCase("VolumeDown", SyntheticKeyCode.VOLUME_DOWN)]
        [TestCase("VolumeUp", SyntheticKeyCode.VOLUME_UP)]
        public void ValidateThatKeyCodeCanBeCorrectlyParsed(string keyCode, SyntheticKeyCode expectedKeyCode)
        {
            var parsedKey = this.syntheticKeyboard.GetSynthethicKeyCodes(keyCode).Single();
            Assert.That(parsedKey, Is.EqualTo(expectedKeyCode));
        }


        [TestCase("Ctrl + C", 2)]
        [TestCase("Ctrl + Alt + Delete", 3)]
        [TestCase("Ctrl + Alt + Delete", 3)]
        [TestCase("1 + + + 2", 3)]
        [TestCase("Num 1 + Num + + Num 2", 3)]
        public void ValidateThatHotKeyCanBeCorrectlyParsed(string keyCode, int expectedKeyCount)
        {
            var parsedKeys = this.syntheticKeyboard.GetSynthethicKeyCodes(keyCode);
            Assert.That(parsedKeys.Count(), Is.EqualTo(expectedKeyCount));
        }

        [TestCase("")]
        [TestCase("╪")]
        public void ValidateThatArgumentExceptionIsThrowIfKeyCanNotBeParsed(string keyCode)
        {
            Assert.Throws<ArgumentException>(() => { this.syntheticKeyboard.GetSynthethicKeyCodes(keyCode); });
        }
    }
}
