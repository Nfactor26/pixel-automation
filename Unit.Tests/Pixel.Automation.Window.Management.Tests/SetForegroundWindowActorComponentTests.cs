using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Drawing;

namespace Pixel.Automation.Window.Management.Tests
{
    class SetForegroundWindowActorComponentTests
    {
        [Test]
        public void ValidThatSetForegroundWindowActorCanBeInitialized()
        {
            var actor = new SetForegroundWindowActorComponent();

            Assert.IsNotNull(actor.ApplicationWindow);          
        }

        [Test]
        public void ValidThatSetForegroundWindowActorCanSetWindowToForeground()
        {

            var window = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);

            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>()).Returns(window);      

            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var actor = new SetForegroundWindowActorComponent()
            {
                EntityManager = entityManager
            };

            actor.Act();

            argumentProcessor.Received(1).GetValue<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>());        
            windowManager.Received(1).SetForeGroundWindow(window);

        }
    }
}
