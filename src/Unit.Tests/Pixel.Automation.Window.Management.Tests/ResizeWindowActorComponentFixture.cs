using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Window.Management.Tests
{
    class ResizeWindowActorComponentFixture
    {
        [Test]
        public void ValidThatResizeWindowActorCanBeInitialized()
        {
            var actor = new ResizeWindowActorComponent();

            Assert.IsNotNull(actor.ApplicationWindow);
            Assert.IsNotNull(actor.Dimension);
        }

        [Test]
        public async Task ValidThatResizeWindowActorCanResizeWindow()
        {

            var window = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", BoundingBox.Empty, true);

            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();
          
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>()).Returns(window);
            argumentProcessor.GetValueAsync<Dimension>(Arg.Any<InArgument<Dimension>>()).Returns(Dimension.ZeroExtents);

            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var actor = new ResizeWindowActorComponent()
            {
                EntityManager = entityManager
            };

            await actor.ActAsync();

            await argumentProcessor.Received(1).GetValueAsync<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>());
            await argumentProcessor.Received(1).GetValueAsync<Dimension>(Arg.Any<InArgument<Dimension>>());
            windowManager.Received(1).SetWindowSize(window, 0, 0);

        }
    }
}
