using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using System.Threading.Tasks;

namespace Pixel.Automation.Input.Devices.Tests
{

    /// <summary>
    /// Use this Mock class for testing functionalities on abstract DeviceInputActor
    /// </summary>
    public class MockInputDeviceActor : InputDeviceActor
    {
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        public override async Task ActAsync()
        {
            await GetScreenCoordinateFromControl(this.TargetControl as InArgument<UIControl>);
        }

        public new bool IsKeySequenceValid(string keySequence)
        {
            return base.IsKeySequenceValid(keySequence);
        }
    }

    class DeviceInputActorTests
    {
        /// <summary>
        /// Validate that screen coordinate can be retrieved from a UIControl that is configured as a InArgument
        /// </summary>
        [Test]
        public async Task ValidateThatScreenCoordinateCanBeRetrievedFromConfiguredControl()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var uiControl = Substitute.For<UIControl>();
            uiControl.GetClickablePointAsync().Returns((100.0, 100.0));
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<UIControl>(Arg.Any<InArgument<UIControl>>()).Returns(uiControl);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var deviceInputActor = new MockInputDeviceActor()
            {
                EntityManager = entityManager,
                TargetControl = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, PropertyPath = "SomeProperty" } //so that Argument.IsConfigured() returns true
            };
            await deviceInputActor.ActAsync();

            await argumentProcessor.Received(1).GetValueAsync<UIControl>(Arg.Any<InArgument<UIControl>>());
            await uiControl.Received(1).GetClickablePointAsync();
        }

        /// <summary>
        /// Validate that when DeviceInputActor is a child of ControlEntity , it can retrieve UIControl from ControlEntity component and then retrieve 
        /// its ScreenCoordinate.
        /// </summary>
        /// <param name="buttonToClick"></param>
        /// <param name="clickMode"></param>
        [Test]
        public async Task ValidateThatScreenCoordinateCanBeRetrievedFromParentControlEntity()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var uiControl = Substitute.For<UIControl>();
            uiControl.GetClickablePointAsync().Returns((100.0, 100.0));

            var controlEntity = Substitute.For<Entity, IControlEntity>();
            (controlEntity as IControlEntity).GetControl().Returns(uiControl);

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);


            var deviceInputActor = new MockInputDeviceActor()
            {
                EntityManager = entityManager,
                Parent = controlEntity
            };

            await deviceInputActor.ActAsync();

            await (controlEntity as IControlEntity).Received(1).GetControl();
            await uiControl.Received(1).GetClickablePointAsync();
        }


        /// <summary>
        /// ConfigurationException shoud be thrown since target control is not configured and control entity is not available
        /// </summary>
        [Test]
        public async Task ValidateThatExceptionIsThrownIfControlEntityIsMissingAndTargetControlIsNotConfigured()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var deviceInputActor = new MockInputDeviceActor()
            {
                EntityManager = entityManager
            };
            Assert.ThrowsAsync<ConfigurationException>(async () => { await deviceInputActor.ActAsync(); });
            await Task.CompletedTask;
        }

        /// <summary>
        /// ElementNotFoundException  shoud be thrown if control could not be retrieved
        /// </summary>
        [Test]
        public async Task ValidateThatExceptionIsThrownIfControlCouldNotBeRetrieved()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<UIControl>(Arg.Any<InArgument<UIControl>>()).Returns(default(UIControl));
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            var deviceInputActor = new MockInputDeviceActor()
            {
                EntityManager = entityManager,
                TargetControl = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, PropertyPath = "SomeProperty" } //so that Argument.IsConfigured() returns true
            };
            Assert.ThrowsAsync<ElementNotFoundException>(async () => { await deviceInputActor.ActAsync(); });
            await Task.CompletedTask;
        }

        [TestCase("Ctrl + C", true)]
        [TestCase("", false)]
        [TestCase("InvalidKey", false)]
        public void VerifyThatKeySequenceCanBeValidated(string keySequence, bool expectedResult)
        {
            var entityManager = Substitute.For<IEntityManager>();
            var syntheticKeyboard = Substitute.For<ISyntheticKeyboard>();
            syntheticKeyboard.GetSynthethicKeyCodes(Arg.Is<string>("Ctrl + C")).Returns(new [] { SyntheticKeyCode.LCONTROL, SyntheticKeyCode.VK_C });
            syntheticKeyboard.GetSynthethicKeyCodes(Arg.Is<string>("")).Returns(new SyntheticKeyCode [] { });
            syntheticKeyboard.GetSynthethicKeyCodes(Arg.Is<string>("InvalidKey")).Returns(new SyntheticKeyCode[] { });
            entityManager.GetServiceOfType<ISyntheticKeyboard>(Arg.Any<string>()).Returns(syntheticKeyboard);
            var deviceInputActor = new MockInputDeviceActor()
            {
                EntityManager = entityManager               
            };
                       
            Assert.AreEqual(expectedResult, deviceInputActor.IsKeySequenceValid(keySequence));
            syntheticKeyboard.Received(1).GetSynthethicKeyCodes(Arg.Is<string>(keySequence));

        }
    }


}
