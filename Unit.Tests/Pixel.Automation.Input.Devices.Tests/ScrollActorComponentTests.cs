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

            Assert.AreEqual(ScrollMode.Vertical, scrollActorComponent.ScrollMode);
            Assert.AreEqual(ScrollDirection.Down, scrollActorComponent.ScrollDirection);

            //validate that up direction can be assigned to vertical scroll mode
            scrollActorComponent.ScrollDirection = ScrollDirection.Up;           
            Assert.AreEqual(ScrollDirection.Up, scrollActorComponent.ScrollDirection);

            //validate that left direction can not be assigned to vertical scroll mode
            scrollActorComponent.ScrollDirection = ScrollDirection.Left;
            Assert.AreNotEqual(ScrollDirection.Left, scrollActorComponent.ScrollDirection);
            Assert.AreEqual(ScrollDirection.Up, scrollActorComponent.ScrollDirection);

            //validate that right direction can not be assigned to vertical scroll mode
            scrollActorComponent.ScrollDirection = ScrollDirection.Right;
            Assert.AreNotEqual(ScrollDirection.Right, scrollActorComponent.ScrollDirection);
            Assert.AreEqual(ScrollDirection.Up, scrollActorComponent.ScrollDirection);

            //validate that changing from vertical scroll mode to horizontal automatically changes direction to right
            scrollActorComponent.ScrollMode = ScrollMode.Horizontal;
            Assert.AreEqual(ScrollDirection.Right, scrollActorComponent.ScrollDirection);

            //validate that left direction can be assigned to horizontal scroll mode
            scrollActorComponent.ScrollDirection = ScrollDirection.Left;
            Assert.AreEqual(ScrollDirection.Left, scrollActorComponent.ScrollDirection);

            //validate that up direction can not be assigned to horizontal scroll mode
            scrollActorComponent.ScrollDirection = ScrollDirection.Up;
            Assert.AreNotEqual(ScrollDirection.Up, scrollActorComponent.ScrollDirection);
            Assert.AreEqual(ScrollDirection.Left, scrollActorComponent.ScrollDirection);

            //validate that down direction can not be assigned to horizontal scroll mode
            scrollActorComponent.ScrollDirection = ScrollDirection.Down;
            Assert.AreNotEqual(ScrollDirection.Down, scrollActorComponent.ScrollDirection);
            Assert.AreEqual(ScrollDirection.Left, scrollActorComponent.ScrollDirection);

        }

        [TestCase(ScrollMode.Horizontal, ScrollDirection.Left)]
        [TestCase(ScrollMode.Horizontal, ScrollDirection.Right)]
        [TestCase(ScrollMode.Vertical, ScrollDirection.Down)]
        [TestCase(ScrollMode.Vertical, ScrollDirection.Up)]
        public void ValidateThatScrollActorCanPerformDifferentCombinationsOfScroll(ScrollMode scrollMode, ScrollDirection scrollDirection)
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
                ScrollMode = scrollMode,
                ScrollDirection = scrollDirection
            };
            scrollActorComponent.Act();

            argumentProcessor.Received(1).GetValue<int>(Arg.Any<InArgument<int>>());

            switch(scrollMode)
            {
                case ScrollMode.Horizontal:
                    synthethicMouse.Received(1).HorizontalScroll(Arg.Any<int>());
                    break;
                case ScrollMode.Vertical:
                    synthethicMouse.Received(1).VerticalScroll(Arg.Any<int>());
                    break;
            }

        }
    }
}
