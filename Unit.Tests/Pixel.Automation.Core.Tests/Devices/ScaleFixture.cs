using NUnit.Framework;
using Pixel.Automation.Core.Devices;

namespace Pixel.Automation.Core.Tests.Devices
{
    class ScaleFixture
    {
        [Test]
        public void ValidateThatScaleCanBeInitialized()
        {
            var scale = new Scale();
            Assert.AreEqual(0, scale.ScaleX);
            Assert.AreEqual(0, scale.ScaleY);

            scale = new Scale(1, 2);
            Assert.AreEqual(1, scale.ScaleX);
            Assert.AreEqual(2, scale.ScaleY);
        }
    }
}
