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
            Assert.That(0, Is.EqualTo(boundingBox.X));
            Assert.That(0, Is.EqualTo(boundingBox.Y));
            Assert.That(800, Is.EqualTo(boundingBox.Width));
            Assert.That(600, Is.EqualTo(boundingBox.Height));
        }
    }
}
