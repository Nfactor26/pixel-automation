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
            Assert.IsNotNull(dataComponent);
            Assert.AreEqual("FakeDataComponent", dataComponent.Name);
            Assert.AreEqual("FakeDataComponent", dataComponent.Tag);

            dataComponent = new FakeDataComponent("Name", "Tag");
            Assert.IsNotNull(dataComponent);
            Assert.AreEqual("Name", dataComponent.Name);
            Assert.AreEqual("Tag", dataComponent.Tag);
        }
    }
}
