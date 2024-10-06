using NUnit.Framework;
using Pixel.Automation.Core.TestData;
using System;
using System.Linq;

namespace Pixel.Automation.Core.Tests.TestData
{
    class TestFixtureFixture
    {
        [Test]
        public void ValidateThatTestFixtureCanBeInitialized()
        {
            var testFixture = new TestFixture();

            Assert.That(string.IsNullOrEmpty(testFixture.FixtureId) == false);
            Assert.That(testFixture.DisplayName is null);
            Assert.That(testFixture.Order, Is.EqualTo(0));
            Assert.That(testFixture.IsMuted == false);
            Assert.That(testFixture.ScriptFile is null);
            Assert.That(testFixture.Description is null);
            Assert.That(testFixture.Tags is not null);
            Assert.That(testFixture.Category, Is.EqualTo("Default"));
            Assert.That(testFixture.Tests is not null);
            Assert.That(testFixture.TestFixtureEntity is null);
            Assert.That(testFixture.ToString(), Is.EqualTo(testFixture.FixtureId));

            testFixture.DisplayName = "TestFixture";
            testFixture.Order = 10;
            testFixture.IsMuted = true;
            testFixture.ScriptFile = "TestFixture.csx";
            testFixture.Description = "Description";
            testFixture.Tags.Add("color", "red");
            testFixture.Tests.Add(new TestCase());
            testFixture.TestFixtureEntity = new Entity();

       
            Assert.That(testFixture.DisplayName, Is.EqualTo("TestFixture"));
            Assert.That(testFixture.Order, Is.EqualTo(10));
            Assert.That(testFixture.IsMuted);
            Assert.That(testFixture.ScriptFile, Is.EqualTo("TestFixture.csx"));
            Assert.That(testFixture.Description, Is.EqualTo("Description"));
            Assert.That("red", Is.EqualTo(testFixture.Tags["color"]));
            Assert.That(testFixture.Tests.Any());
            Assert.That(testFixture.TestFixtureEntity is not null);
        }

        [Test]
        public void ValidateThatTestFixtureCanBeCloned()
        {
            var testFixture = new TestFixture() { DisplayName = "TestCase", Order = 10, IsMuted = true, ScriptFile = "ScriptFile.csx", 
                Description = "Description", Category = "GroupOne", TestFixtureEntity = new Entity() };
            testFixture.Tags.Add("color", "red");
            testFixture.Tests.Add(new TestCase());
            var copyOftestFixture = testFixture.Clone() as TestFixture;

            Assert.That(copyOftestFixture.DisplayName, Is.EqualTo(testFixture.DisplayName));
            Assert.That(copyOftestFixture.Order, Is.EqualTo(testFixture.Order));
            Assert.That(copyOftestFixture.Description, Is.EqualTo(testFixture.Description));
            Assert.That(copyOftestFixture.IsMuted, Is.EqualTo(testFixture.IsMuted));
            Assert.That("red", Is.EqualTo(copyOftestFixture.Tags["color"]));
            Assert.That(copyOftestFixture.Category, Is.EqualTo(testFixture.Category)    );          
            Assert.That(copyOftestFixture.ScriptFile, Is.Not.EqualTo(testFixture.ScriptFile));      
            Assert.That(copyOftestFixture.TestFixtureEntity, Is.Not.EqualTo(testFixture.TestFixtureEntity));
            Assert.That(testFixture.Tests.SequenceEqual(copyOftestFixture.Tests) == false);

        }
    }
}
