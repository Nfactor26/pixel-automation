using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Core.Tests
{
    public class FakeComponent : Component
    {
        public FakeComponent() : base()
        {

        }

        public FakeComponent(string name, string tag) : base(name, tag)
        {

        }
    }

    [TestFixture]
    public class ComponentFixture
    {
        /// <summary>
        /// Validate that Component can be initialized and default values are as expected
        /// </summary>
        [TestCase]
        public void ValidateThatComponentCanBeInitialized()
        {
            var component = new FakeComponent("Component", "Tag");
            Assert.That(component is not null);

            Assert.That(component.Name, Is.EqualTo("Component"));
            Assert.That(component.Tag, Is.EqualTo("Tag"));
            Assert.That(!string.IsNullOrEmpty(component.Id));
            Assert.That(component.IsEnabled);
            Assert.That(component.ProcessOrder, Is.EqualTo(1));
            Assert.That(component.IsValid);
            Assert.That(component.EntityManager is null);
            Assert.That(component.Parent is null);
        }

        /// <summary>
        /// Validate that properties can be set and their values can be retrieved from a component
        /// </summary>
        [TestCase]
        public void ValidateThatPropertiesCanBeSetAndRetrieved()
        {
            var component = new FakeComponent("Component", "Tag");
            
            component.Name = "FakeComponent";
            Assert.That(component.Name, Is.EqualTo("FakeComponent"));

            component.Tag = "FakeTag";
            Assert.That(component.Tag, Is.EqualTo("FakeTag"));

            component.ProcessOrder = 10;
            Assert.That(component.ProcessOrder, Is.EqualTo(10));

            component.Parent = new Entity();
            Assert.That(component.Parent is not null);

            var entityManager = Substitute.For<IEntityManager>();
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            component.EntityManager = entityManager;
            Assert.That(component.EntityManager is not null);
            Assert.That(component.ArgumentProcessor is not null);
        }

        /// <summary>
        /// When ValidateComponent() is invoked on Component, Component's IsValid state should be updated
        /// </summary>
        [TestCase("", "", false)]
        [TestCase("Name", "", false)]
        [TestCase("", "Tag", false)]
        [TestCase("Name", "Tag", true)]
        public void ValidateThatComponentsValidateMethodCanDetermineWhetherComponentIsValid(string name, string tag, bool expectedValidState)
        {
            var component = new FakeComponent() { Name = name, Tag = tag };
            Assert.That(component.IsValid); //Initially all components are invalid
            component.ValidateComponent();
            Assert.That(component.IsValid, Is.EqualTo(expectedValidState));
        }

        /// <summary>
        /// Validate that ToString() correctly returns the overridden representation
        /// </summary>
        [TestCase]
        public void ValidateThatToStringReturnsCorrectValue()
        {
            var component = new FakeComponent("Name", "Tag");
            string expectedValue = "Component -> Name:Name|Tag:Tag|IsEnabled:True";
            Assert.That(component.ToString(), Is.EqualTo(expectedValue));
        }
    }
}
