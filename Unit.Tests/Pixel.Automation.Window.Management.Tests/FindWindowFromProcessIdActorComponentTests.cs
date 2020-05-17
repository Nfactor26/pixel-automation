using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Drawing;

namespace Pixel.Automation.Window.Management.Tests
{
    class FindWindowFromProcessIdActorComponentTests
    {
        [Test]
        public void ValidThatFindWindowFromProcessIdActorCanBeInitialized()
        {
            var actor = new FindWindowFromProcessIdActorComponent();

            Assert.IsNotNull(actor.ProcessId);
            Assert.IsFalse(actor.ProcessId.CanChangeType);
            Assert.IsNotNull(actor.FoundWindow);

        }

        [Test]
        public void ValidateThatFindWindowFromProcessIdActorCanLocateWindow()
        {
            var window = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);

            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();
            windowManager.FromProcessId(Arg.Any<int>()).Returns(window);

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<int>(Arg.Any<InArgument<int>>()).Returns(0);

            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var actor = new FindWindowFromProcessIdActorComponent()
            {
                EntityManager = entityManager
            };

            actor.Act();

            argumentProcessor.Received(1).GetValue<int>(Arg.Any<InArgument<int>>());
            windowManager.Received(1).FromProcessId(0);
            argumentProcessor.Received(1).SetValue<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), window);
        }
    }
}
