using NUnit.Framework;

namespace Pixel.Automation.Editor.Core.Tests
{
    public class PropertyMapFixture
    {
        [Test]
        public void ValidateThatPropertyMapCanBeInitizlied()
        {
            var propertyMap = new PropertyMap()
            {
                AssignFrom = typeof(int).Name,
                AssignFromType = typeof(int),
                AssignTo = typeof(string).Name,
                AssignToType = typeof(string)
            };

            Assert.That(propertyMap.AssignFrom, Is.EqualTo(typeof(int).Name));
            Assert.That(propertyMap.AssignFromType, Is.EqualTo(typeof(int)));
            Assert.That(propertyMap.AssignTo, Is.EqualTo(typeof(string).Name));
            Assert.That(propertyMap.AssignToType, Is.EqualTo(typeof(string)));
        }
    }
}
