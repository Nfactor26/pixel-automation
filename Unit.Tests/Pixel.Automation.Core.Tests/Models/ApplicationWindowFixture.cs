using NUnit.Framework;
using Pixel.Automation.Core.Models;
using System;
using System.Drawing;

namespace Pixel.Automation.Core.Tests.Models
{
    class ApplicationWindowFixture
    {
        [Test]
        public void ValidateThatApplicationWindowCanBeInitialized()
        {
            var applicationWindow = new ApplicationWindow(0, IntPtr.Zero, "WindowTitle", Rectangle.Empty, false);
            Assert.AreEqual(0, applicationWindow.ProcessId);
            Assert.AreEqual(IntPtr.Zero, applicationWindow.HWnd);
            Assert.AreEqual("WindowTitle", applicationWindow.WindowTitle);
            Assert.AreEqual(Rectangle.Empty, applicationWindow.WindowBounds);
            Assert.AreEqual(false, applicationWindow.IsVisible);

            applicationWindow.ProcessId = 1;
            applicationWindow.HWnd = new IntPtr(1);
            applicationWindow.WindowTitle = "NewWindowTitle";
            applicationWindow.WindowBounds = new Rectangle(0, 0, 600, 800);
            applicationWindow.IsVisible = true;

            Assert.AreEqual(1, applicationWindow.ProcessId);
            Assert.AreEqual(new IntPtr(1), applicationWindow.HWnd);
            Assert.AreEqual("NewWindowTitle", applicationWindow.WindowTitle);
            Assert.AreEqual(new Rectangle(0, 0, 600, 800), applicationWindow.WindowBounds);
            Assert.AreEqual(true, applicationWindow.IsVisible);


        }
    }
}
