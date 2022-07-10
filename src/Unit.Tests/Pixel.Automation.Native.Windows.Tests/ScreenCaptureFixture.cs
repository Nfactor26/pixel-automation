using NUnit.Framework;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Native.Windows;
using System.Drawing;
using System.IO;
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
            var screenshot = screenCapture.CaptureDesktop();
            using(var ms = new MemoryStream(screenshot))
            {
                using (var bitmap = Image.FromStream(ms))
                {
                    Assert.AreEqual(resX, bitmap.Width);
                    Assert.AreEqual(resY, bitmap.Height);
                }
            }           
        }

        /// <summary>
        /// Validate that ScreenCapture can capture specified bounds on desktop
        /// </summary>
        [Test]
        public void ValidateThatSpecifiedDesktopAreaCanBeCaptured()
        {
            ScreenCapture screenCapture = new ScreenCapture();
            var screenshot = screenCapture.CaptureArea(new BoundingBox() { X = 100, Y = 100, Width = 400, Height = 600 });
            using (var ms = new MemoryStream(screenshot))
            {
                using (var bitmap = Image.FromStream(ms))
                {
                    Assert.AreEqual(400, bitmap.Width);
                    Assert.AreEqual(600, bitmap.Height);
                }
            }           
        }
    }
}