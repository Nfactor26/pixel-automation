using NUnit.Framework;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Models;
using System;

namespace Pixel.Automation.Core.Tests.Models
{
    class ApplicationWindowFixture
    {
        [Test]
        public void ValidateThatApplicationWindowCanBeInitialized()
        {
            var applicationWindow = new ApplicationWindow(0, IntPtr.Zero, "WindowTitle", BoundingBox.Empty, false);
            Assert.That(0, Is.EqualTo(applicationWindow.ProcessId));
            Assert.That(IntPtr.Zero, Is.EqualTo(applicationWindow.HWnd));
            Assert.That("WindowTitle", Is.EqualTo(applicationWindow.WindowTitle));
            Assert.That(BoundingBox.Empty, Is.EqualTo(applicationWindow.WindowBounds));
            Assert.That(false, Is.EqualTo(applicationWindow.IsVisible));

            applicationWindow.ProcessId = 1;
            applicationWindow.HWnd = new IntPtr(1);
            applicationWindow.WindowTitle = "NewWindowTitle";
            applicationWindow.WindowBounds = new BoundingBox(0, 0, 600, 800);
            applicationWindow.IsVisible = true;

            Assert.That(1, Is.EqualTo(applicationWindow.ProcessId));
            Assert.That(new IntPtr(1), Is.EqualTo(applicationWindow.HWnd));
            Assert.That("NewWindowTitle", Is.EqualTo(applicationWindow.WindowTitle));
            Assert.That(new BoundingBox(0, 0, 600, 800), Is.EqualTo(applicationWindow.WindowBounds));
            Assert.That(true, Is.EqualTo(applicationWindow.IsVisible));


        }
    }
}
