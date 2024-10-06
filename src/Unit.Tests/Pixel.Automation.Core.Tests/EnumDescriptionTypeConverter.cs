using NUnit.Framework;
using Pixel.Automation.Core.Devices;

namespace Pixel.Automation.Core.Tests
{
    class EnumDescriptionTypeConverterFixture
    {
        [TestCase(MouseButton.LeftButton, "Left Button")]
        [TestCase(null, "")]
        public void ValidateThatEnumValuesCanBeConvertedToFriendlyNamesbyConverter(object value, string expectedDescription)
        {
            var enumDescriptionConverter = new EnumDescriptionTypeConverter(value?.GetType() ?? typeof(object));
            var description = enumDescriptionConverter.ConvertTo(null, null, value, typeof(string));

            Assert.That(description, Is.EqualTo(expectedDescription));
        }
    }
}
