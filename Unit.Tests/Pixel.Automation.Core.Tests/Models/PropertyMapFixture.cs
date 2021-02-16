using NUnit.Framework;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Core.Tests.Models
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

            Assert.AreEqual(typeof(int).Name, propertyMap.AssignFrom);
            Assert.AreEqual(typeof(int), propertyMap.AssignFromType);
            Assert.AreEqual(typeof(string).Name, propertyMap.AssignTo);
            Assert.AreEqual(typeof(string), propertyMap.AssignToType);
        }
    }
}
