using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Core.Tests.Args
{
    class ComponentRemovedEventArgsFixture
    {
        [Test]
        public void ValidateThaComponentRemovedEventArgsCanBeInitialized()
        {
            var componentRemovedEventArgs = new ComponentRemovedEventArgs(Substitute.For<IComponent>(), new Entity());
            Assert.IsNotNull(componentRemovedEventArgs.RemovedComponent);
            Assert.IsNotNull(componentRemovedEventArgs.RemovedFromEntity);
        }
    }
}
