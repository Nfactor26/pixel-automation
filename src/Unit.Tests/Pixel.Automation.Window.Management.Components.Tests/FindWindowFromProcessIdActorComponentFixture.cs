﻿using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Window.Management.Components.Tests
{
    class FindWindowFromProcessIdActorComponentFixture
    {
        [Test]
        public void ValidThatFindWindowFromProcessIdActorCanBeInitialized()
        {
            var actor = new FindWindowFromProcessIdActorComponent();

            Assert.That(actor.ProcessId is not null);
            Assert.That(actor.ProcessId.CanChangeType == false);
            Assert.That(actor.FoundWindow is not null);

        }

        [Test]
        public async Task ValidateThatFindWindowFromProcessIdActorCanLocateWindow()
        {
            var window = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", BoundingBox.Empty, true);

            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();
            windowManager.FromProcessId(Arg.Any<int>()).Returns(window);

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<int>(Arg.Any<InArgument<int>>()).Returns(0);

            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var actor = new FindWindowFromProcessIdActorComponent()
            {
                EntityManager = entityManager
            };

            await actor.ActAsync();

            await argumentProcessor.Received(1).GetValueAsync<int>(Arg.Any<InArgument<int>>());
            windowManager.Received(1).FromProcessId(0);
            await argumentProcessor.Received(1).SetValueAsync<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), window);
        }
    }
}
