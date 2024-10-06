using NUnit.Framework;
using Pixel.Automation.Core.Devices;

namespace Pixel.Automation.Core.Tests.Devices
{
    class DimensionFixture
    {
        /// <summary>
        /// Validate that Dimension object can be initialized property with correct values passes as parameters
        /// </summary>
        [Test]
        public void ValidateThatDimensionCanBeInitialized()
        {
            var dimension = new Dimension(800, 600);
            Assert.That(dimension.Width, Is.EqualTo(800));
            Assert.That(dimension.Height, Is.EqualTo(600));
        }

        /// <summary>
        /// Validate that ZeroExtents static property returns a Dimension with Width and Height both set to 0
        /// </summary>
        [Test]
        public void ValidateThatZeroExtentsStaticPropertyRepresentsZeroDimension()
        {
            var dimension = Dimension.ZeroExtents;
            Assert.That(dimension.Width, Is.EqualTo(0));
            Assert.That(dimension.Height, Is.EqualTo(0));
        }
    }
}
