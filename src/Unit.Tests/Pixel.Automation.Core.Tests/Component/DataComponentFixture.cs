using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pixel.Automation.Core.Tests
{
    public class FakeDataComponent : DataComponent
    {
        public FakeDataComponent() : base()
        {

        }

        public FakeDataComponent(string name, string tag): base(name, tag)
        {

        }
    }

    [TestFixture]
    public class DataComponentFixture
    {
        [Test]
        public void ValidateThatDataComponentCanBeInitialized()
        {
            var dataComponent = new FakeDataComponent();
            Assert.That(dataComponent is not null);
            Assert.That(dataComponent.Name, Is.EqualTo("FakeDataComponent"));
            Assert.That(dataComponent.Tag, Is.EqualTo("FakeDataComponent"));

            dataComponent = new FakeDataComponent("Name", "Tag");
            Assert.That(dataComponent is not null);
            Assert.That(dataComponent.Name, Is.EqualTo("Name"));
            Assert.That(dataComponent.Tag, Is.EqualTo("Tag"));
        }
    }
}
