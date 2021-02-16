using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
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

        [Test]
        /// <summary>
        /// Verify that Validate component can identify configuration issue when Target mode is set to Empty and Drag location is not configured
        /// </summary>
        public void VerifyThatValidateComponetCanIdentifyIfMoveToArgumentIsNotConfiguredForTargetEmptyMode()
        {
            var mouseDragActor = new MouseDragActorComponent()
            {
                Target = Target.Empty
            };

            //MoveTo is already initialized with a screen coordinate
            Assert.AreEqual(true, mouseDragActor.ValidateComponent());

            //We are initializing MoveTo again but leaivng out the default value of screen coordinate.
            //Default instance of ScreenCoordinate is automatically created as the default value
            mouseDragActor.DragPoint = new InArgument<ScreenCoordinate>();
            Assert.AreEqual(true, mouseDragActor.ValidateComponent());
        }

        [Test]
        /// <summary>
        /// Verify that Validate component can identify any configuration issue when Target mode is set to Control and TargetControl is configured
        /// </summary>
        public void VerifyThatValidateComponetCanIdentifyIfTargetControlIsNotConfiguredForTargetControlMode()
        {
            var mouseDragActor = new MouseDragActorComponent()
            {
                Target = Target.Control
            };
            Assert.AreEqual(false, mouseDragActor.ValidateComponent());

            //set the property path to correctly configure TargetControl in data bound mode
            mouseDragActor.TargetControl.PropertyPath = "VariablePointingToUIControl";
            Assert.AreEqual(true, mouseDragActor.ValidateComponent());
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

            var mouseDragActor = new MouseDragActorComponent()
            {
                EntityManager = entityManager,
                Target = Target.Control
            };
            Assert.AreEqual(false, mouseDragActor.ValidateComponent());

            //Now we add the control entity
            mouseDragActor.Parent = controlEntity;
            Assert.AreEqual(true, mouseDragActor.ValidateComponent());

        }
    }
}
