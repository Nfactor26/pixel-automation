using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Core.Tests.Args
{
    class ComponentAddedEventArgsFixture
    {
        [Test]
        public void ValidateThatComponentAddedEventArgsCanBeInitialized()
        {
            var componentAddedEventArgs = new ComponentAddedEventArgs(Substitute.For<IComponent>(), new Entity());
            Assert.IsNotNull(componentAddedEventArgs.AddedComponent);
            Assert.IsNotNull(componentAddedEventArgs.AddedToEntity);
        }
    }
}
