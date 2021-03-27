using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Input.Devices.Tests
{
    class ScrollActorComponentTests
    {
        [Test]
        public void ValidateThatScrollActorComponentCanBeInitialized()
        {
            var scrollActorComponent = new ScrollActorComponent();
            
            Assert.AreEqual(ScrollDirection.Down, scrollActorComponent.ScrollDirection);

            //validate that direction can be changed
            scrollActorComponent.ScrollDirection = ScrollDirection.Up;           
            Assert.AreEqual(ScrollDirection.Up, scrollActorComponent.ScrollDirection);        
          
        }

        [TestCase(ScrollDirection.Left)]
        [TestCase(ScrollDirection.Right)]
        [TestCase(ScrollDirection.Down)]
        [TestCase(ScrollDirection.Up)]
        public void ValidateThatScrollActorCanPerformDifferentCombinationsOfScroll(ScrollDirection scrollDirection)
        {
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<int>(Arg.Any<InArgument<int>>()).Returns(10);

            var synthethicMouse = Substitute.For<ISyntheticMouse>();

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<ISyntheticMouse>().Returns(synthethicMouse);

            var scrollActorComponent = new ScrollActorComponent()
            {
                EntityManager = entityManager,              
                ScrollDirection = scrollDirection
            };
            scrollActorComponent.Act();

            argumentProcessor.Received(1).GetValue<int>(Arg.Any<InArgument<int>>());

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
