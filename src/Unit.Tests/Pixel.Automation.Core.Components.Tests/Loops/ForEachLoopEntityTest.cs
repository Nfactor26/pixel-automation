using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components.Loops;
using Pixel.Automation.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Automation.Core.Components.Tests
{
    public class ForEachLoopEntityTest
    {
        /// <summary>
        /// Vadidate that foreach loop entity loops for each item in enumerable sequence
        /// </summary>
        [Test]
        public void ValidateThatForEachEntityLoopsForEachItemInEnumerableSequence()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            var enumerableCollection = new List<int>() { 1, 2 };
            argumentProcessor.GetValueAsync<List<int>>(Arg.Any<Argument>()).Returns(enumerableCollection);
            argumentProcessor.When(x => x.SetValueAsync<int>(Arg.Any<Argument>(), Arg.Any<int>())).Do(x => { });
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var forEachLoopEntity = new ForEachLoopEntity()
            {
                TargetCollection = new InArgument<List<int>>(),
                Current = new InArgument<int>()
            };

            forEachLoopEntity.EntityManager = entityManager;
            forEachLoopEntity.ResolveDependencies();
            var placeHolderEntity = forEachLoopEntity.Components[0] as PlaceHolderEntity;
            Assert.That(placeHolderEntity is not null);

            var actorComponent = Substitute.For<ActorComponent>();
            placeHolderEntity.AddComponent(actorComponent);

            var iterationResult = forEachLoopEntity.GetNextComponentToProcess().ToList();

            Assert.That(iterationResult.Count, Is.EqualTo(2));
            Assert.That(iterationResult.ElementAt(0), Is.EqualTo(actorComponent));
            Assert.That(iterationResult.ElementAt(1), Is.EqualTo(actorComponent));

            argumentProcessor.Received(1).GetValueAsync<List<int>>(Arg.Any<Argument>());
            argumentProcessor.Received(2).SetValueAsync<int>(Arg.Any<Argument>(), Arg.Any<int>());
        }


        /// <summary>
        /// Vadidate that foreach loop entity loops for each item in enumerable sequence
        /// </summary>
        [Test]
        public void ValidateThatForEachEntityLoopsCanWorkWithNestedLoops()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            var enumerableCollection = new List<int>() { 1, 2 };
            argumentProcessor.GetValueAsync<List<int>>(Arg.Any<Argument>()).Returns(enumerableCollection);
            argumentProcessor.When(x => x.SetValueAsync<int>(Arg.Any<Argument>(), Arg.Any<int>())).Do(x => { });
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var forEachLoopEntity = new ForEachLoopEntity()
            {
                TargetCollection = new InArgument<List<int>>(),
                Current = new InArgument<int>()
            };

            forEachLoopEntity.EntityManager = entityManager;
            forEachLoopEntity.ResolveDependencies();
            var placeHolderEntity = forEachLoopEntity.Components[0] as PlaceHolderEntity;
            Assert.That(placeHolderEntity is not null);

            var actorComponent = Substitute.For<ActorComponent>();
            var nestedLoopEntity = Substitute.For<Entity, ILoop>();
            //Entity.IsEnabled and ILoop.IsEnabled are differnt. We need to typecast to ILoop and mock it
            //This is because GetNextComponentProcess checks IComponent.IsEnabled to decide if component should be processed.
            (nestedLoopEntity as ILoop).IsEnabled.Returns(true);

            placeHolderEntity.AddComponent(nestedLoopEntity);
            nestedLoopEntity.GetNextComponentToProcess().Returns(new List<IComponent>() { actorComponent });
            nestedLoopEntity.Components.Returns(new List<IComponent>() { actorComponent });

            var iterationResult = forEachLoopEntity.GetNextComponentToProcess().ToList();

            Assert.That(iterationResult.Count, Is.EqualTo(6));
            Assert.That(iterationResult.ElementAt(0), Is.EqualTo(nestedLoopEntity));
            Assert.That(iterationResult.ElementAt(1), Is.EqualTo(actorComponent));
            Assert.That(iterationResult.ElementAt(2), Is.EqualTo(nestedLoopEntity));
            Assert.That(iterationResult.ElementAt(3), Is.EqualTo(nestedLoopEntity));
            Assert.That(iterationResult.ElementAt(4), Is.EqualTo(actorComponent));
            Assert.That(iterationResult.ElementAt(5), Is.EqualTo(nestedLoopEntity));

            nestedLoopEntity.Received(2).ResetComponent();
        }
    }
}
