using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using System.Drawing;

namespace Pixel.Automation.Core.Tests.Models
{
    class ScrapedControlFixture
    {
        [Test]
        public void ValidetThatScrapedControlCanBeInitializedAndDisposed()
        {
            IControlIdentity controlIdentity = Substitute.For<IControlIdentity>();
            var controlImage = new Bitmap(800, 600);
            var scapedControl = new ScrapedControl() { ControlData = controlIdentity, ControlImage = controlImage };

            Assert.AreSame(controlIdentity, scapedControl.ControlData);
            Assert.AreSame(controlImage, scapedControl.ControlImage);

            scapedControl.Dispose();
        }

    }
}
