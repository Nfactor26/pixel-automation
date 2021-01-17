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
            Assert.IsNotNull(component);

            Assert.AreEqual("Component", component.Name);
            Assert.AreEqual("Tag", component.Tag);
            Assert.IsNotEmpty(component.Id);
            Assert.IsTrue(component.IsEnabled);
            Assert.AreEqual(1, component.ProcessOrder);
            Assert.IsTrue(component.IsValid);
            Assert.IsNull(component.EntityManager);
            Assert.IsNull(component.Parent);
        }

        /// <summary>
        /// Validate that properties can be set and their values can be retrieved from a component
        /// </summary>
        [TestCase]
        public void ValidateThatPropertiesCanBeSetAndRetrieved()
        {
            var component = new FakeComponent("Component", "Tag");
            
            component.Name = "FakeComponent";
            Assert.AreEqual("FakeComponent", component.Name);

            component.Tag = "FakeTag";
            Assert.AreEqual("FakeTag", component.Tag);

            component.ProcessOrder = 10;
            Assert.AreEqual(10, component.ProcessOrder);

            component.Parent = new Entity();
            Assert.IsNotNull(component.Parent);

            var entityManager = Substitute.For<IEntityManager>();
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            component.EntityManager = entityManager;
            Assert.IsNotNull(component.EntityManager);
            Assert.IsNotNull(component.ArgumentProcessor);
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
            Assert.IsTrue(component.IsValid); //Initially all components are invalid
            component.ValidateComponent();
            Assert.AreEqual(expectedValidState, component.IsValid);
        }

        /// <summary>
        /// Validate that ToString() correctly returns the overridden representation
        /// </summary>
        [TestCase]
        public void ValidateThatToStringReturnsCorrectValue()
        {
            var component = new FakeComponent("Name", "Tag");
            string expectedValue = "Component -> Name:Name|Tag:Tag|IsEnabled:True";
            Assert.AreEqual(expectedValue, component.ToString());
        }
    }
}
