using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;


namespace Pixel.Automation.Input.Devices.Tests
{
    class MouseOverActorComponentTests
    {
        /// <summary>
        /// Validate that MouseOverActor can move mouse to configured coordinates
        /// </summary>
        [Test]
        public void ValidateThatMouseOverActorComponentCanMoveCursorToConfiugredCoordinates()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<ScreenCoordinate>(Arg.Any<InArgument<ScreenCoordinate>>()).Returns(new ScreenCoordinate(100, 100));

            var synthethicMouse = Substitute.For<ISyntheticMouse>();

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<ISyntheticMouse>().Returns(synthethicMouse);

            var mouseOverActor = new MouseOverActorComponent()
            {
                Target = Target.Empty,          
                EntityManager = entityManager
            };

            mouseOverActor.Act();

            argumentProcessor.Received(1).GetValue<ScreenCoordinate>(Arg.Any<InArgument<ScreenCoordinate>>());
            synthethicMouse.Received(1).MoveMouseTo(Arg.Any<ScreenCoordinate>(), SmoothMode.Interpolated);           
        }
    }
}
