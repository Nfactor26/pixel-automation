using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components.Waits;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Core.Components.Tests
{
    public class DelayActorComponentTest
    {
        [Test]
        public void ValidateThatDelayActorComponentCanRun()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<double>(Arg.Any<Argument>()).Returns(1);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var delayActorComponent = new DelayActorComponent();
            delayActorComponent.EntityManager = entityManager;

            delayActorComponent.Act(); //should internally sleep for 1 sec. Believe in Thread.Sleep.

            argumentProcessor.Received(1).GetValue<double>(Arg.Any<Argument>());
        }
    }
}
