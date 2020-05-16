using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Interfaces;
using System.Linq;

namespace Pixel.Automation.Core.Components.Tests
{
    public class TestCaseEntitiesTest
    {
        [Test]
        public void VerifyThatOneTimeSetupEntityCanBeInitialized()
        {
            var entity = new OneTimeSetUpEntity();
            entity.ResolveDependencies();

            Assert.AreEqual("One Time SetUp", entity.Name);
            Assert.AreEqual("OneTimeSetUp", entity.Tag);
            Assert.AreEqual(0, entity.Components.Count());
        }

        [Test]
        public void VerifyThatOneTimeTearDownEntityCanBeInitialized()
        {
            var entity = new OneTimeTearDownEntity();
            entity.ResolveDependencies();

            Assert.AreEqual("One Time TearDown", entity.Name);
            Assert.AreEqual("OneTimeTearDown", entity.Tag);
            Assert.AreEqual(0, entity.Components.Count());
        }

        [Test]
        public void VerifyThatSetUpEntityCanBeInitialized()
        {
            var entity = new SetUpEntity();
            entity.ResolveDependencies();

            Assert.AreEqual("SetUp", entity.Name);
            Assert.AreEqual("SetUp", entity.Tag);
            Assert.AreEqual(0, entity.Components.Count());
        }

        [Test]
        public void VerifyThatTearDownEntityCanBeInitialized()
        {
            var entity = new TearDownEntity();
            entity.ResolveDependencies();

            Assert.AreEqual("Tear Down", entity.Name);
            Assert.AreEqual("TearDown", entity.Tag);
            Assert.AreEqual(0, entity.Components.Count());
        }

        [Test]
        public void VerifyThatTestSequenceEntityCanBeInitialized()
        {
            var entity = new TestSequenceEntity();
            entity.ResolveDependencies();

            Assert.AreEqual("Test Sequence", entity.Name);
            Assert.AreEqual("TestSequence", entity.Tag);
            Assert.AreEqual(0, entity.Components.Count());
        }

        [Test]
        public void VerifyThatTestFixtureEntityCanBeInitialized()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var entity = new TestFixtureEntity() { EntityManager = entityManager };
            entity.ResolveDependencies();
          
            Assert.AreEqual("Test Fixture", entity.Name);
            Assert.AreEqual("TestFixture", entity.Tag);
            Assert.AreEqual(2, entity.Components.Count());

        }

        [Test]
        public void VerifyThatTestCaseEntityCanBeInitialized()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var entity = new TestCaseEntity() { EntityManager = entityManager };
            entity.ResolveDependencies();

            Assert.AreEqual("Test Case", entity.Name);
            Assert.AreEqual("TestCase", entity.Tag);
            Assert.AreEqual(3, entity.Components.Count());

        }
    }
}
