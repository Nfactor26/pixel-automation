using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;

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

        /// <summary>
        /// Validate that MouseOverActor can move mouse to configured control
        /// </summary>
        [Test]
        public void ValidateThatMouseOverActorComponentCanMoveCursorToConfiugredControl()
        {
            var entityManager = Substitute.For<IEntityManager>();

            UIControl uiControl = Substitute.For<UIControl>();
            uiControl.When(x => x.GetClickablePoint(out Arg.Any<double>(), out Arg.Any<double>())).Do(x =>
            {
                x[0] = 100.0;
                x[1] = 100.0;
            });
          
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<UIControl>(Arg.Any<InArgument<UIControl>>()).Returns(uiControl);

            var synthethicMouse = Substitute.For<ISyntheticMouse>();

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<ISyntheticMouse>().Returns(synthethicMouse);

            var mouseOverActor = new MouseOverActorComponent()
            {
                Target = Target.Control,
                TargetControl = new InArgument<UIControl>() { Mode = ArgumentMode.Default, DefaultValue = uiControl  },
                EntityManager = entityManager
            };

            mouseOverActor.Act();

            argumentProcessor.Received(1).GetValue<UIControl>(Arg.Any<InArgument<UIControl>>());
            uiControl.Received(1).GetClickablePoint(out Arg.Any<double>(), out Arg.Any<double>());
            synthethicMouse.Received(1).MoveMouseTo(Arg.Any<ScreenCoordinate>(), SmoothMode.Interpolated);
        }

        
        [Test]
        /// <summary>
        /// Verify that Validate component can identify configuration issue when Target mode is set to Empty and MoveTo location is not configured
        /// </summary>
        public void VerifyThatValidateComponetCanIdentifyIfMoveToArgumentIsNotConfiguredForTargetEmptyMode()
        {
            var mouseOverActor = new MouseOverActorComponent()
            {
                Target = Target.Empty
            };
            
            //MoveTo is already initialized with a screen coordinate
            Assert.AreEqual(true, mouseOverActor.ValidateComponent());

            //We are initializing MoveTo again but leaivng out the default value of screen coordinate.
            //Default instance of ScreenCoordinate is automatically created as the default value
            mouseOverActor.MoveTo = new InArgument<ScreenCoordinate>();
            Assert.AreEqual(true, mouseOverActor.ValidateComponent());
        }

        [Test]
        /// <summary>
        /// Verify that Validate component can identify any configuration issue when Target mode is set to Control and TargetControl is configured
        /// </summary>
        public void VerifyThatValidateComponetCanIdentifyIfTargetControlIsNotConfiguredForTargetControlMode()
        {
            var mouseOverActor = new MouseOverActorComponent()
            {
                Target = Target.Control
            };                    
            Assert.AreEqual(false, mouseOverActor.ValidateComponent());

            //set the property path to correctly configure TargetControl in data bound mode
            mouseOverActor.TargetControl.PropertyPath = "VariablePointingToUIControl";
            Assert.AreEqual(true, mouseOverActor.ValidateComponent());
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

            var mouseOverActor = new MouseOverActorComponent()
            {
                EntityManager = entityManager,
                Target = Target.Control
            };
            Assert.AreEqual(false, mouseOverActor.ValidateComponent());

            //Now we add the control entity
            mouseOverActor.Parent = controlEntity;
            Assert.AreEqual(true, mouseOverActor.ValidateComponent());

        }
    }
}
