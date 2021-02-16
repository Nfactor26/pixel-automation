using NUnit.Framework;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Core.Tests.Models
{
    class PropertyDescriptionFixture
    {
        [Test]
        public void ValidateThatPropertyDescriptionCanBeInitialized()
        {
            var propertyDescription = new PropertyDescription("Address", typeof(string));
            Assert.AreEqual("Address", propertyDescription.PropertyName);
            Assert.AreEqual(typeof(string), propertyDescription.PropertyType);
        }
    }
}
