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
            Assert.That("Address", Is.EqualTo(propertyDescription.PropertyName));
            Assert.That(typeof(string), Is.EqualTo(propertyDescription.PropertyType));
        }
    }
}
