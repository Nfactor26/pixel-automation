using NUnit.Framework;
using Pixel.Automation.UIA.Components.Enums;
using Pixel.Windows.Automation;

namespace Pixel.Automation.UIA.Components.Tests
{
    public class Tests
    {
     
        [Test]
        public void ValidateThatControlTypeCanBeConvertedToWinControlType()
        {
            WinControlType winControlType = ControlType.Button.ToWinControlType();
            Assert.That(winControlType, Is.EqualTo(WinControlType.Button));
        }

        [TestCase(WinControlType.Button)]
        public void ValidatethatWinControlTypeCanBeConvertedToControlType(WinControlType winControlType)
        {
            ControlType controlType = winControlType.ToUIAControlType();
            Assert.That(controlType.ProgrammaticName, Is.EqualTo($"ControlType.{winControlType}"));
        }
    }
}