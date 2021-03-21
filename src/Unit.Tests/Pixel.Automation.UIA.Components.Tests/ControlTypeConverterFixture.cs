extern alias uiaComWrapper;
using NUnit.Framework;
using Pixel.Automation.UIA.Components.Enums;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.Tests
{
    public class Tests
    {
     
        [Test]
        public void ValidateThatControlTypeCanBeConvertedToWinControlType()
        {
            WinControlType winControlType = ControlType.Button.ToWinControlType();
            Assert.AreEqual(WinControlType.Button, winControlType);
        }

        [TestCase(WinControlType.Button)]
        public void ValidatethatWinControlTypeCanBeConvertedToControlType(WinControlType winControlType)
        {
            ControlType controlType = winControlType.ToUIAControlType();
            Assert.AreEqual($"ControlType.{winControlType}", controlType.ProgrammaticName);
        }
    }
}