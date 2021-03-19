using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Drawing;

namespace Pixel.Automation.Window.Management.Tests
{
    class FindWindowFromHandleActorComponentFixture
    {

        [Test]
        public void ValidThatFindWindowFromHandleActorCanBeInitialized()
        {
            var actor = new FindWindowFromHandleActorComponent();

            Assert.IsNotNull(actor.WindowHandle);
            Assert.IsFalse(actor.WindowHandle.CanChangeType);
            Assert.IsNotNull(actor.FoundWindow);
        
        }

        [Test]
        public void ValidateThatFindWindowFromHandleActorCanLocateWindowWithAGivenHandle()
        {
            var window = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);
         
            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();
            windowManager.FromHwnd(Arg.Any<IntPtr>()).Returns(window);

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<IntPtr>(Arg.Any<InArgument<IntPtr>>()).Returns(IntPtr.Zero);

            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var actor = new FindWindowFromHandleActorComponent()
            {
                EntityManager = entityManager
            };

            actor.Act();

            argumentProcessor.Received(1).GetValue<IntPtr>(Arg.Any<InArgument<IntPtr>>());
            windowManager.Received(1).FromHwnd(IntPtr.Zero);
            argumentProcessor.Received(1).SetValue<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), window);
        }
    }
}
