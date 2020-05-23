using Nish26.Automation.Input.Devices;
using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Input.Devices.Tests
{
    class MouseDragActorComponentTests
    {

        [Test]
        public void ValidateThatDragDropActorComponentCanPerformDrag()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<ScreenCoordinate>(Arg.Any<InArgument<ScreenCoordinate>>()).Returns(new ScreenCoordinate(100, 100));

            var synthethicMouse = Substitute.For<ISyntheticMouse>();

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<ISyntheticMouse>().Returns(synthethicMouse);

            var dragActorComponent = new MouseDragActorComponent()
            {
                EntityManager = entityManager,
                DragMode = DragMode.From,
                Target = Target.Empty,
                SmootMode = SmoothMode.None
            };

            dragActorComponent.Act();

            argumentProcessor.Received(1).GetValue<ScreenCoordinate>(Arg.Any<InArgument<ScreenCoordinate>>());
            synthethicMouse.Received(1).MoveMouseTo(Arg.Any<ScreenCoordinate>(), SmoothMode.None);
            synthethicMouse.Received(1).ButtonDown(MouseButton.LeftButton);

        }


        [Test]
        public void ValidateThatDragDropActorComponentCanPerformDrop()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<ScreenCoordinate>(Arg.Any<InArgument<ScreenCoordinate>>()).Returns(new ScreenCoordinate(100, 100));

            var synthethicMouse = Substitute.For<ISyntheticMouse>();

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<ISyntheticMouse>().Returns(synthethicMouse);
            
            var dragActorComponent = new MouseDragActorComponent()
            {
                EntityManager = entityManager,
                DragMode = DragMode.To,
                Target = Target.Empty,
                SmootMode = SmoothMode.None
            };

            dragActorComponent.Act();

            argumentProcessor.Received(1).GetValue<ScreenCoordinate>(Arg.Any<InArgument<ScreenCoordinate>>());
            synthethicMouse.Received(1).MoveMouseTo(Arg.Any<ScreenCoordinate>(), SmoothMode.None);
            synthethicMouse.Received(1).ButtonUp(MouseButton.LeftButton);
        }
    }
}
