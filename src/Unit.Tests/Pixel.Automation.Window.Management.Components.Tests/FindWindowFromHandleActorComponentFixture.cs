using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Window.Management.Components.Tests
{
    class FindWindowFromHandleActorComponentFixture
    {

        [Test]
        public void ValidThatFindWindowFromHandleActorCanBeInitialized()
        {
            var actor = new FindWindowFromHandleActorComponent();

            Assert.That(actor.WindowHandle is not null);
            Assert.That(actor.WindowHandle.CanChangeType  == false);
            Assert.That(actor.FoundWindow is not null);
        
        }

        [Test]
        public async Task ValidateThatFindWindowFromHandleActorCanLocateWindowWithAGivenHandle()
        {
            var window = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", BoundingBox.Empty, true);
         
            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();
            windowManager.FromHwnd(Arg.Any<IntPtr>()).Returns(window);

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<IntPtr>(Arg.Any<InArgument<IntPtr>>()).Returns(IntPtr.Zero);

            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var actor = new FindWindowFromHandleActorComponent()
            {
                EntityManager = entityManager
            };

            await actor.ActAsync();

            await argumentProcessor.Received(1).GetValueAsync<IntPtr>(Arg.Any<InArgument<IntPtr>>());
            windowManager.Received(1).FromHwnd(IntPtr.Zero);
            await argumentProcessor.Received(1).SetValueAsync<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), window);
        }
    }
}
