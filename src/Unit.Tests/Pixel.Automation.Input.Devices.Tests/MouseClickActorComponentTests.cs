using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;
using System.Threading.Tasks;

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
        public async Task ValidateThatMouseClickCanBePerformedAtConfiguredCoordinates(MouseButton buttonToClick, ClickMode clickMode)
        {
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<ScreenCoordinate>(Arg.Any<InArgument<ScreenCoordinate>>()).Returns(new ScreenCoordinate(100, 100));

            var synthethicMouse = Substitute.For<ISyntheticMouse>();

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<ISyntheticMouse>().Returns(synthethicMouse);

            var mouseClickActor = new MouseClickActorComponent()
            {
                Target = Target.Point,
                ClickMode = clickMode,
                Button = buttonToClick,
                EntityManager = entityManager
            };

            await mouseClickActor.ActAsync();

            argumentProcessor.Received(1).GetValueAsync<ScreenCoordinate>(Arg.Any<InArgument<ScreenCoordinate>>());
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

        /// <summary>
        /// Validate that MouseClick can be performed on configured control
        /// </summary>
        [Test]
        public async Task ValidateThatMouseClickCanBePerformedAtContifugredControl()
        {
            UIControl uiControl = Substitute.For<UIControl>();
            uiControl.GetClickablePointAsync().Returns((100.0, 100.0));
           
            var entityManager = Substitute.For<IEntityManager>();
            var controlEntity = Substitute.For<Entity, IControlEntity>();
            controlEntity.EntityManager = entityManager;
            (controlEntity as IControlEntity).GetControl().Returns(uiControl);         

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            var synthethicMouse = Substitute.For<ISyntheticMouse>();

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<ISyntheticMouse>().Returns(synthethicMouse);

            var mouseClickActor = new MouseClickActorComponent()
            {
                Target = Target.Control,             
                EntityManager = entityManager
            };
            mouseClickActor.Parent = controlEntity;

            await mouseClickActor.ActAsync();

            argumentProcessor.Received(0).GetValueAsync<UIControl>(Arg.Any<InArgument<UIControl>>());
            await uiControl.Received(1).GetClickablePointAsync();
            synthethicMouse.Received(1).MoveMouseTo(Arg.Any<ScreenCoordinate>(), SmoothMode.Interpolated);
        }

        [Test]
        /// <summary>
        /// Verify that Validate component can identify configuration issue when Target mode is set to Empty and MoveTo location is not configured
        /// </summary>
        public void VerifyThatValidateComponetCanIdentifyIfClickAtArgumentIsNotConfiguredForTargetEmptyMode()
        {
            var mouseClickActor = new MouseClickActorComponent()
            {
                Target = Target.Point
            };

            //MoveTo is already initialized with a screen coordinate
            Assert.AreEqual(true, mouseClickActor.ValidateComponent());

            //We are initializing MoveTo again but leaivng out the default value of screen coordinate.
            //Default instance of ScreenCoordinate is automatically created as the default value
            mouseClickActor.ClickAt = new InArgument<ScreenCoordinate>();
            Assert.AreEqual(true, mouseClickActor.ValidateComponent());
        }

        [Test]
        /// <summary>
        /// Verify that Validate component can identify any configuration issue when Target mode is set to Control and TargetControl is configured
        /// </summary>
        public void VerifyThatValidateComponetCanIdentifyIfTargetControlIsNotConfiguredForTargetControlMode()
        {
            var mouseClickActor = new MouseClickActorComponent()
            {
                Target = Target.Control
            };
            Assert.AreEqual(false, mouseClickActor.ValidateComponent());

            //set the property path to correctly configure TargetControl in data bound mode
            mouseClickActor.TargetControl.PropertyPath = "VariablePointingToUIControl";
            Assert.AreEqual(true, mouseClickActor.ValidateComponent());
        }

        [Test]
        /// <summary>
        /// Verify that Validate component can identify any configuration issue when Target mode is set to Control and Parent Control Entity is configured
        /// </summary>
        public void VerifyThatValidateComponetCanIdentifyIfControlDetailsNotConfiguredForTargetControlMode()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var controlEntity = Substitute.For<Entity, IControlEntity>();
            controlEntity.EntityManager = entityManager;

            var mouseClickActor = new MouseClickActorComponent()
            {
                EntityManager = entityManager,
                Target = Target.Control
            };
            Assert.AreEqual(false, mouseClickActor.ValidateComponent());

            //Now we add the control entity
            mouseClickActor.Parent = controlEntity;
            Assert.AreEqual(true, mouseClickActor.ValidateComponent());

        }
    }
}
