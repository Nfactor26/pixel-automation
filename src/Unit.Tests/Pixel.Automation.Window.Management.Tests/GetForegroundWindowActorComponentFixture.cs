using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Pixel.Automation.Window.Management.Tests
{
    class GetForegroundWindowActorComponentFixture
    {

        [Test]
        public void ValidThatGetForegroundWindowActorCanBeInitialized()
        {
            var actor = new GetForegroundWindowActorComponent();

            Assert.IsNotNull(actor.ForeGroundWindow);
        }

        [Test]
        public async Task ValidThatGetForegroundWindowActorCanLocateForegroundWindow()
        {

            var window = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);

            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();
            windowManager.GetForeGroundWindow().Returns(window);
        
            var argumentProcessor = Substitute.For<IArgumentProcessor>();         

            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var actor = new GetForegroundWindowActorComponent()
            {
                EntityManager = entityManager
            };

            await actor.ActAsync();

            windowManager.Received(1).GetForeGroundWindow();
            await argumentProcessor.Received(1).SetValueAsync<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), window);

        }

    }
}
