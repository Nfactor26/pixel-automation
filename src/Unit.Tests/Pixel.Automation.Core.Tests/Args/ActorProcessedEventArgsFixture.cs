using NSubstitute;
using NUnit.Framework;

namespace Pixel.Automation.Core.Tests.Args
{
    class ActorProcessedEventArgsFixture
    {
        [Test]
        public void ValidateThatActorProcessedEventArgsCanBeInitialized()
        {
            var actorProcessedEventArgs = new ActorProcessedEventArgs(Substitute.For<ActorComponent>(), true);
            Assert.IsNotNull(actorProcessedEventArgs.ProcessedActor);
            Assert.IsTrue(actorProcessedEventArgs.IsSuccess);
        }
    }
}
