using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Input.Devices.Tests
{
    class MouseClickActorComponentTests
    {

        /// <summary>
        /// Configure the MouseClickActor to click at a given coordinate and validate that different combinations of mouse button and mouse click (single and double)
        /// can be performed.
        /// </summary>
        /// <param name="buttonToClick"></param>
        /// <param name="clickMode"></param>
        [TestCase(MouseButton.LeftButton, ClickMode.SingleClick)]
        [TestCase(MouseButton.LeftButton, ClickMode.DoubleClick)]
        [TestCase(MouseButton.MiddleButton, ClickMode.SingleClick)]
        [TestCase(MouseButton.MiddleButton, ClickMode.DoubleClick)]
        [TestCase(MouseButton.RightButton, ClickMode.SingleClick)]
        [TestCase(MouseButton.RightButton, ClickMode.DoubleClick)]
        public void ValidateThatMouseClickCanBePerformedAtConfiguredCoordinates(MouseButton buttonToClick, ClickMode clickMode)
        {
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<ScreenCoordinate>(Arg.Any<InArgument<ScreenCoordinate>>()).Returns(new ScreenCoordinate(100, 100));

            var synthethicMouse = Substitute.For<ISyntheticMouse>();

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<ISyntheticMouse>().Returns(synthethicMouse);

            var mouseClickActor = new MouseClickActorComponent()
            {
                Target = Target.Empty,
                ClickMode = clickMode,
                Button = buttonToClick,
                EntityManager = entityManager
            };

            mouseClickActor.Act();

            argumentProcessor.Received(1).GetValue<ScreenCoordinate>(Arg.Any<InArgument<ScreenCoordinate>>());
            synthethicMouse.Received(1).MoveMouseTo(Arg.Any<ScreenCoordinate>(), SmoothMode.Interpolated);
            switch(clickMode)
            {
                case ClickMode.SingleClick:
                    synthethicMouse.Received(1).Click(buttonToClick);
                    break;
                case ClickMode.DoubleClick:
                    synthethicMouse.Received(1).DoubleClick(buttonToClick);
                    break;
            }

        }        
    }
}
