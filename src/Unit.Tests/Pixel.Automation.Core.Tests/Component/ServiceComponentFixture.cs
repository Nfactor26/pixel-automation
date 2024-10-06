using NUnit.Framework;

namespace Pixel.Automation.Core.Tests
{
    public class FakeServiceComponent : ServiceComponent
    {
        public FakeServiceComponent() : base()
        {

        }

        public FakeServiceComponent(string name, string tag) : base(name, tag)
        {

        }
    }

    [TestFixture]
    public class ServiceComponentFixture
    {
        [Test]
        public void ValidateThatServiceComponentCanBeInitialized()
        {
            var serviceComponent = new FakeServiceComponent();
            Assert.That(serviceComponent is not null);
            Assert.That(serviceComponent.Name, Is.EqualTo("FakeServiceComponent"));
            Assert.That(serviceComponent.Tag, Is.EqualTo("FakeServiceComponent"));

            serviceComponent = new FakeServiceComponent("Name", "Tag");
            Assert.That(serviceComponent is not null);
            Assert.That(serviceComponent.Name, Is.EqualTo("Name"));
            Assert.That(serviceComponent.Tag, Is.EqualTo("Tag"));
        }
    }
}
