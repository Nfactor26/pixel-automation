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

            Assert.That(entity.Name, Is.EqualTo("One Time SetUp"));
            Assert.That(entity.Tag, Is.EqualTo("OneTimeSetUp"));
            Assert.That(entity.Components.Count(), Is.EqualTo(0));
        }

        [Test]
        public void VerifyThatOneTimeTearDownEntityCanBeInitialized()
        {
            var entity = new OneTimeTearDownEntity();
            entity.ResolveDependencies();

            Assert.That(entity.Name, Is.EqualTo("One Time TearDown"));
            Assert.That(entity.Tag, Is.EqualTo("OneTimeTearDown"));
            Assert.That(entity.Components.Count(), Is.EqualTo(0));
        }

        [Test]
        public void VerifyThatSetUpEntityCanBeInitialized()
        {
            var entity = new SetUpEntity();
            entity.ResolveDependencies();

            Assert.That(entity.Name, Is.EqualTo("SetUp"));
            Assert.That(entity.Tag, Is.EqualTo("SetUp"));
            Assert.That(entity.Components.Count(), Is.EqualTo(0));
        }

        [Test]
        public void VerifyThatTearDownEntityCanBeInitialized()
        {
            var entity = new TearDownEntity();
            entity.ResolveDependencies();

            Assert.That(entity.Name, Is.EqualTo("Tear Down"));
            Assert.That(entity.Tag, Is.EqualTo("TearDown"));
            Assert.That(entity.Components.Count(), Is.EqualTo(0));
        }     

        [Test]
        public void VerifyThatTestFixtureEntityCanBeInitialized()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var entity = new TestFixtureEntity() { EntityManager = entityManager };
            entity.ResolveDependencies();
          
            Assert.That(entity.Name, Is.EqualTo("Test Fixture"));
            Assert.That(entity.Tag, Is.EqualTo("TestFixture"));
            Assert.That(entity.Components.Count(), Is.EqualTo(4));

        }

        [Test]
        public void VerifyThatTestCaseEntityCanBeInitialized()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var entity = new TestCaseEntity() { EntityManager = entityManager };
            entity.ResolveDependencies();

            Assert.That(entity.Name, Is.EqualTo("Test Case"));
            Assert.That(entity.Tag, Is.EqualTo("TestCase"));
            Assert.That(entity.Components.Count(), Is.EqualTo(0));

        }
    }
}
