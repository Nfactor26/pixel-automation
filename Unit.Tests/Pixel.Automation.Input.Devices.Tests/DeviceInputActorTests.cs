using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Input.Devices.Tests
{

    /// <summary>
    /// Use this Mock class for testing functionalities on abstract DeviceInputActor
    /// </summary>
    public class MockDeviceInputActor : DeviceInputActor
    {
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        public override void Act()
        {
            _ = GetScreenCoordinateFromControl(this.TargetControl as InArgument<UIControl>);
        }
    }

    class DeviceInputActorTests
    {
        /// <summary>
        /// Validate that screen coordinate can be retrieved from a UIControl that is configured as a InArgument
        /// </summary>
        [Test]
        public void ValidateThatScreenCoordinateCanBeRetrieevedFromConfiguredControl()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var uiControl = Substitute.For<UIControl>();
            uiControl.When(x => x.GetClickablePoint(out Arg.Any<double>(), out Arg.Any<double>())).Do
            (x =>
            {
               x[0] = 100.0;
               x[1] = 100.0;
            });

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<UIControl>(Arg.Any<InArgument<UIControl>>()).Returns(uiControl);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var deviceInputActor = new MockDeviceInputActor()
            {
                EntityManager = entityManager,
                TargetControl = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, PropertyPath = "SomeProperty" } //so that Argument.IsConfigured() returns true
            };
            deviceInputActor.Act();

            argumentProcessor.Received(1).GetValue<UIControl>(Arg.Any<InArgument<UIControl>>());
            uiControl.Received(1).GetClickablePoint(out Arg.Any<double>(), out Arg.Any<double>());
        }

        /// <summary>
        /// Validate that when DeviceInputActor is a child of ControlEntity , it can retrieve UIControl from ControlEntity component and then retrieve 
        /// its ScreenCoordinate.
        /// </summary>
        /// <param name="buttonToClick"></param>
        /// <param name="clickMode"></param>
        [Test]
        public void ValidateThatScreenCoordinateCanBeRetrievedFromParentControlEntity()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var uiControl = Substitute.For<UIControl>();
            uiControl.When(x => x.GetClickablePoint(out Arg.Any<double>(), out Arg.Any<double>())).Do
            (x => {
                x[0] = 100.0;
                x[1] = 100.0;
            });

            var controlEntity = Substitute.For<Entity, IControlEntity>();
            (controlEntity as IControlEntity).GetControl().Returns(uiControl);

            var argumentProcessor = Substitute.For<IArgumentProcessor>();        
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
        

            var deviceInputActor = new MockDeviceInputActor()
            {               
                EntityManager = entityManager,
                Parent = controlEntity
            };

            deviceInputActor.Act();

            (controlEntity as IControlEntity).Received(1).GetControl();
            uiControl.Received(1).GetClickablePoint(out Arg.Any<double>(), out Arg.Any<double>());
        }
    }


}
