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
            Assert.AreEqual(800, dimension.Width);
            Assert.AreEqual(600, dimension.Height);
        }

        /// <summary>
        /// Validate that ZeroExtents static property returns a Dimension with Width and Height both set to 0
        /// </summary>
        [Test]
        public void ValidateThatZeroExtentsStaticPropertyRepresentsZeroDimension()
        {
            var dimension = Dimension.ZeroExtents;
            Assert.AreEqual(0, dimension.Width);
            Assert.AreEqual(0, dimension.Height);
        }
    }
}
