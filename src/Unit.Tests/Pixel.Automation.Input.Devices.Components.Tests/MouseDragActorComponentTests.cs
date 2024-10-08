﻿using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;
using System.Threading.Tasks;

namespace Pixel.Automation.Input.Devices.Components.Tests
{
    class MouseDragActorComponentTests
    {

        [Test]
        public async Task ValidateThatDragDropActorComponentCanPerformDragDropWithTargetPoints()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<ScreenCoordinate>(Arg.Any<InArgument<ScreenCoordinate>>()).Returns(new ScreenCoordinate(100, 100));

            var synthethicMouse = Substitute.For<ISyntheticMouse>();

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<ISyntheticMouse>().Returns(synthethicMouse);

            var dragActorComponent = new DragDropActorComponent()
            {
                EntityManager = entityManager,               
                Target = Target.Point,
                SmootMode = SmoothMode.None
            };

            await dragActorComponent.ActAsync();

            await argumentProcessor.Received(2).GetValueAsync<ScreenCoordinate>(Arg.Any<InArgument<ScreenCoordinate>>());
            synthethicMouse.Received(2).MoveMouseTo(Arg.Any<ScreenCoordinate>(), SmoothMode.None);
            synthethicMouse.Received(1).ButtonDown(MouseButton.LeftButton);
            synthethicMouse.Received(1).ButtonUp(MouseButton.LeftButton);

        }

              
        [Test]
        /// <summary>
        /// Verify that Validate component can identify configuration issue when Target mode is set to Empty and Drag location is not configured
        /// </summary>
        public void VerifyThatValidateComponetCanIdentifyIfMoveToArgumentIsNotConfiguredForTargetPointMode()
        {
            var mouseDragActor = new DragDropActorComponent()
            {
                Target = Target.Point
            };

            //DragStartPoint and DropEndPoint are already configured with defaults
            Assert.That(mouseDragActor.ValidateComponent(), Is.EqualTo(true));            
        }

        [Test]
        /// <summary>
        /// Verify that Validate component can identify any configuration issue when Target mode is set to Control and TargetControl is configured
        /// </summary>
        public void VerifyThatValidateComponetCanIdentifyIfTargetControlIsNotConfiguredForTargetControlMode()
        {
            var mouseDragActor = new DragDropActorComponent()
            {
                Target = Target.Control
            };
            Assert.That(mouseDragActor.ValidateComponent(), Is.EqualTo(false));

            //set the property path to correctly configure TargetControl in data bound mode
            mouseDragActor.SourceControl.PropertyPath = "VariableOnePointingToUIControl";
            mouseDragActor.TargetControl.PropertyPath = "VariableTwoPointingToUIControl";
            Assert.That(mouseDragActor.ValidateComponent(), Is.EqualTo(true));
        }
    }
}
