using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;
using System.Drawing;
using System.Threading.Tasks;

namespace Pixel.Automation.Input.Devices.Tests
{
    class MouseMoveByActorComponentTests
    {
       
        [Test]
        public async Task ValidateThatMouseMoveByActorCanMoveCursorByConfiugredAmount()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<Point>(Arg.Any<InArgument<Point>>()).Returns(new Point(100, 100));

            var synthethicMouse = Substitute.For<ISyntheticMouse>();

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<ISyntheticMouse>().Returns(synthethicMouse);

            var mouseMoveByActor = new MouseMoveByActorComponent()
            {               
                EntityManager = entityManager
            };

            await mouseMoveByActor.ActAsync();

            await argumentProcessor.Received(1).GetValueAsync<Point>(Arg.Any<InArgument<Point>>());
            synthethicMouse.Received(1).MoveMouseBy(Arg.Is<int>(100), Arg.Is<int>(100),  SmoothMode.Interpolated);
        }
    }
}
