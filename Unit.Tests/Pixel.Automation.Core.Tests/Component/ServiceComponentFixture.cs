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
            Assert.IsNotNull(serviceComponent);
            Assert.AreEqual("FakeServiceComponent", serviceComponent.Name);
            Assert.AreEqual("FakeServiceComponent", serviceComponent.Tag);

            serviceComponent = new FakeServiceComponent("Name", "Tag");
            Assert.IsNotNull(serviceComponent);
            Assert.AreEqual("Name", serviceComponent.Name);
            Assert.AreEqual("Tag", serviceComponent.Tag);
        }
    }
}
