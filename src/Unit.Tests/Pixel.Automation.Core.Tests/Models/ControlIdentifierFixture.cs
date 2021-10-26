using NUnit.Framework;
using Pixel.Automation.Core.Controls;

namespace Pixel.Automation.Core.Tests.Models
{
    class ControlIdentifierFixture
    {
        [Test]
        public void ValidateThatControlIdentifierCanbeInitialized()
        {
            var controlIdentifier = new ControlIdentifier("id", "controlId");
            Assert.AreEqual("id", controlIdentifier.AttributeName);
            Assert.AreEqual("controlId", controlIdentifier.AttributeValue);
        }
    }
}
