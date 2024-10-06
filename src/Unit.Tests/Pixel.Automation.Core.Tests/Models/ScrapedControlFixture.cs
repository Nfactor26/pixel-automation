using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Core.Tests.Models
{
    class ScrapedControlFixture
    {
        [Test]
        public void ValidetThatScrapedControlCanBeInitializedAndDisposed()
        {
            IControlIdentity controlIdentity = Substitute.For<IControlIdentity>();
            var controlImage = new byte[10];
            var scapedControl = new ScrapedControl() { ControlData = controlIdentity, ControlImage = controlImage };

            Assert.That(controlIdentity, Is.SameAs(scapedControl.ControlData));
            Assert.That(controlImage, Is.SameAs(scapedControl.ControlImage));
        }

    }
}
