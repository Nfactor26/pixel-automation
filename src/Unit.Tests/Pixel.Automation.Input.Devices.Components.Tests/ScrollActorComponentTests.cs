﻿using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;
using System.Threading.Tasks;

namespace Pixel.Automation.Input.Devices.Components.Tests
{
    class ScrollActorComponentTests
    {
        [Test]
        public void ValidateThatScrollActorComponentCanBeInitialized()
        {
            var scrollActorComponent = new ScrollActorComponent();
            
            Assert.That(scrollActorComponent.ScrollDirection, Is.EqualTo(ScrollDirection.Down));

            //validate that direction can be changed
            scrollActorComponent.ScrollDirection = ScrollDirection.Up;           
            Assert.That(scrollActorComponent.ScrollDirection, Is.EqualTo(ScrollDirection.Up));        
          
        }

        [TestCase(ScrollDirection.Left)]
        [TestCase(ScrollDirection.Right)]
        [TestCase(ScrollDirection.Down)]
        [TestCase(ScrollDirection.Up)]
        public async Task ValidateThatScrollActorCanPerformDifferentCombinationsOfScroll(ScrollDirection scrollDirection)
        {
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<int>(Arg.Any<InArgument<int>>()).Returns(10);

            var synthethicMouse = Substitute.For<ISyntheticMouse>();

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<ISyntheticMouse>().Returns(synthethicMouse);

            var scrollActorComponent = new ScrollActorComponent()
            {
                EntityManager = entityManager,              
                ScrollDirection = scrollDirection
            };
            await scrollActorComponent.ActAsync();

            await argumentProcessor.Received(1).GetValueAsync<int>(Arg.Any<InArgument<int>>());

            switch(scrollDirection)
            {
                case ScrollDirection.Left:
                case ScrollDirection.Right:
                    synthethicMouse.Received(1).HorizontalScroll(Arg.Any<int>());
                    break;
                case ScrollDirection.Down:
                case ScrollDirection.Up:
                    synthethicMouse.Received(1).VerticalScroll(Arg.Any<int>());
                    break;
            }

        }
    }
}
