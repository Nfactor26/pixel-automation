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
            Assert.AreEqual(0, screenCoordinate.XCoordinate);
            Assert.AreEqual(0, screenCoordinate.YCoordinate);
            Assert.AreEqual("(0, 0)", screenCoordinate.ToString());

            screenCoordinate = new ScreenCoordinate(100, 100);
            Assert.AreEqual(100, screenCoordinate.XCoordinate);
            Assert.AreEqual(100, screenCoordinate.YCoordinate);
            Assert.AreEqual("(100, 100)", screenCoordinate.ToString());


            screenCoordinate = new ScreenCoordinate(200.0, 200.0);
            Assert.AreEqual(200, screenCoordinate.XCoordinate);
            Assert.AreEqual(200, screenCoordinate.YCoordinate);
            Assert.AreEqual("(200, 200)", screenCoordinate.ToString());
        }
    }
}
