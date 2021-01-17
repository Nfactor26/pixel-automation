using NUnit.Framework;
using Pixel.Automation.Native.Windows;
using System.Windows.Forms;

namespace Pixel.Automation.Nativew.Windows.Tests
{
    public class ScreenCaptureFixture
    {        
        /// <summary>
        /// Validate that ScreenCapture can capture complete desktop snapshot
        /// </summary>
        [Test]
        public void ValidateThatDesktopCanBeCaptured()
        {  
            int resX = Screen.PrimaryScreen.Bounds.Width;
            int resY = Screen.PrimaryScreen.Bounds.Height;
            ScreenCapture screenCapture = new ScreenCapture();
            using(var desktop = screenCapture.CaptureDesktop())
            {
                Assert.AreEqual(resX, desktop.Width);
                Assert.AreEqual(resY, desktop.Height);
            }
        }

        /// <summary>
        /// Validate that ScreenCapture can capture specified bounds on desktop
        /// </summary>
        [Test]
        public void ValidateThatSpecifiedDesktopAreaCanBeCaptured()
        {
            ScreenCapture screenCapture = new ScreenCapture();
            using (var area = screenCapture.CaptureArea(new System.Drawing.Rectangle() { X = 100, Y = 100, Width = 400, Height = 600 }))
            {
                Assert.AreEqual(400, area.Width);
                Assert.AreEqual(600, area.Height);
            }
        }
    }
}