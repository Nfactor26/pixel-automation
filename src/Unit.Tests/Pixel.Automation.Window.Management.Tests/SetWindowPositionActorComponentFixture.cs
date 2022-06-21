using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Pixel.Automation.Window.Management.Tests
{
    class SetWindowPositionActorComponentFixture
    {
        [Test]
        public void ValidThatSetWindwPositionActorCanBeInitialized()
        {
            var actor = new SetWindowPositionActorComponent();

            Assert.IsNotNull(actor.ApplicationWindow);
            Assert.IsNotNull(actor.Position);
        }

        [Test]
        public async Task ValidThatSetForegroundWindowActorCanSetWindowToForeground()
        {

            var window = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);

            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>()).Returns(window);
            var newWindowPosition = new ScreenCoordinate(100, 100);
            argumentProcessor.GetValueAsync<ScreenCoordinate>(Arg.Any<InArgument<ScreenCoordinate>>()).Returns(newWindowPosition);

            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var actor = new SetWindowPositionActorComponent()
            {
                EntityManager = entityManager
            };

            await actor.ActAsync();

            argumentProcessor.Received(1).GetValueAsync<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>());
            windowManager.Received(1).SetWindowPosition(window, newWindowPosition);

        }
    }
}
