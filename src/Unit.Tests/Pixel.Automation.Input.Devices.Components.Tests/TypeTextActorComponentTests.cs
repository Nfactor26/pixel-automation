using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;
using System.Threading.Tasks;

namespace Pixel.Automation.Input.Devices.Components.Tests
{
    class TypeTextActorComponentTests
    {
        [Test]
        public async Task ValidateThatTypeTextActorCanTypeText()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var inputText  = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = "How you doing?" };

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<string>(Arg.Is<InArgument<string>>(inputText)).Returns(inputText.DefaultValue);

            var syntheticKeyboard = Substitute.For<ISyntheticKeyboard>();

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<ISyntheticKeyboard>().Returns(syntheticKeyboard);

            var typeTextActor = new TypeTextActorComponent()
            {           
                Input = inputText,
                EntityManager = entityManager
            };
            await typeTextActor.ActAsync();

            await argumentProcessor.Received(1).GetValueAsync<string>(Arg.Any<InArgument<string>>());
            syntheticKeyboard.Received(1).TypeText("How you doing?");
        }
    }
}
