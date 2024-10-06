using NUnit.Framework;
using Pixel.Automation.Core.Devices;

namespace Pixel.Automation.Core.Tests.Devices
{
    class ScreenCoordinateFixture
    {
        [Test]
        public void ValidateThatScreenCoordinateCanBeInitialized()
        {
            var screenCoordinate = new ScreenCoordinate();
            Assert.That(screenCoordinate.XCoordinate, Is.EqualTo(0));
            Assert.That(screenCoordinate.YCoordinate, Is.EqualTo(0));
            Assert.That(screenCoordinate.ToString(), Is.EqualTo("(0, 0)"));

            screenCoordinate = new ScreenCoordinate(100, 100);
            Assert.That(screenCoordinate.XCoordinate, Is.EqualTo(100));
            Assert.That(screenCoordinate.YCoordinate, Is.EqualTo(100));
            Assert.That(screenCoordinate.ToString(), Is.EqualTo("(100, 100)"));


            screenCoordinate = new ScreenCoordinate(200.0, 200.0);
            Assert.That(screenCoordinate.XCoordinate, Is.EqualTo(200));
            Assert.That(screenCoordinate.YCoordinate, Is.EqualTo(200));
            Assert.That(screenCoordinate.ToString(), Is.EqualTo("(200, 200)"));
        }
    }
}
