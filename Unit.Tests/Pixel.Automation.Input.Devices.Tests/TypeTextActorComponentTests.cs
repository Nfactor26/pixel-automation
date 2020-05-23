using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;


namespace Pixel.Automation.Input.Devices.Tests
{
    class TypeTextActorComponentTests
    {
        [Test]
        public void ValidateThatTypeTextActorCanTypeText()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<string>(Arg.Any<InArgument<string>>()).Returns("How you doing?");

            var syntheticKeyboard = Substitute.For<ISyntheticKeyboard>();

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<ISyntheticKeyboard>().Returns(syntheticKeyboard);

            var typeTextActor = new TypeTextActorComponent()
            {              
                EntityManager = entityManager
            };
            typeTextActor.Act();

            argumentProcessor.Received(1).GetValue<string>(Arg.Any<InArgument<string>>());
            syntheticKeyboard.Received(1).TypeText("How you doing?");
        }
    }
}
