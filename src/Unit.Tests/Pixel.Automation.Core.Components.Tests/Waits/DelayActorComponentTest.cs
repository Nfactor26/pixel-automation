using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components.Waits;
using Pixel.Automation.Core.Interfaces;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Tests
{
    public class DelayActorComponentTest
    {
        [Test]
        public async Task ValidateThatDelayActorComponentCanRun()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<double>(Arg.Any<Argument>()).Returns(1);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var delayActorComponent = new DelayActorComponent();
            delayActorComponent.EntityManager = entityManager;

            await delayActorComponent.ActAsync(); //should internally sleep for 1 sec. Believe in Thread.Sleep.

            argumentProcessor.Received(1).GetValueAsync<double>(Arg.Any<Argument>());
        }
    }
}
