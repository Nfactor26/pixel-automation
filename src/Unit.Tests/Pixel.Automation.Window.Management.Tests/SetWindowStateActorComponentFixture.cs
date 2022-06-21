﻿using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Pixel.Automation.Window.Management.Tests
{
    class SetWindowStateActorComponentFixture
    {
        [Test]
        public void ValidThatSetWindwStateActorCanBeInitialized()
        {
            var actor = new SetWindowStateActorComponent();

            Assert.IsNotNull(actor.ApplicationWindow);
            Assert.AreEqual(WindowState.Maximize, actor.DesiredState);
        }

        [Test]
        public async Task ValidThatSetForegroundWindowActorCanSetWindowToForeground()
        {

            var window = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);

            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>()).Returns(window);
         
            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var actor = new SetWindowStateActorComponent()
            {
                EntityManager = entityManager
            };

            await actor.ActAsync();

            argumentProcessor.Received(1).GetValueAsync<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>());
            windowManager.Received(1).SetWindowState(window, WindowState.Maximize, false);

        }
    }
}
