using NUnit.Framework;
using Pixel.Automation.Core.Controls;

namespace Pixel.Automation.Core.Tests.Models
{
    class BoundingBoxFixture
    {
        [Test]
        public void ValidateThatBoundingBoxCanBeInitialized()
        {
            var boundingBox = new BoundingBox(0, 0, 800, 600);
            Assert.AreEqual(0, boundingBox.X);
            Assert.AreEqual(0, boundingBox.Y);
            Assert.AreEqual(800, boundingBox.Width);
            Assert.AreEqual(600, boundingBox.Height);
        }
    }
}
