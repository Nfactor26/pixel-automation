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
            argumentProcessor.GetValue<List<int>>(Arg.Any<Argument>()).Returns(enumerableCollection);
            argumentProcessor.When(x => x.SetValue<int>(Arg.Any<Argument>(), Arg.Any<int>())).Do(x => { });
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var forEachLoopEntity = new ForEachLoopEntity()
            {
                TargetCollection = new InArgument<List<int>>(),
                Current = new InArgument<int>()
            };

            forEachLoopEntity.EntityManager = entityManager;
            forEachLoopEntity.ResolveDependencies();

            var actorComponent = Substitute.For<ActorComponent>();
            forEachLoopEntity.AddComponent(actorComponent);

            var iterationResult = forEachLoopEntity.GetNextComponentToProcess().ToList();

            Assert.AreEqual(2, iterationResult.Count);
            Assert.AreEqual(actorComponent, iterationResult.ElementAt(0));
            Assert.AreEqual(actorComponent, iterationResult.ElementAt(1));

            argumentProcessor.Received(1).GetValue<List<int>>(Arg.Any<Argument>());
            argumentProcessor.Received(2).SetValue<int>(Arg.Any<Argument>(), Arg.Any<int>());
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
            argumentProcessor.GetValue<List<int>>(Arg.Any<Argument>()).Returns(enumerableCollection);
            argumentProcessor.When(x => x.SetValue<int>(Arg.Any<Argument>(), Arg.Any<int>())).Do(x => { });
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var forEachLoopEntity = new ForEachLoopEntity()
            {
                TargetCollection = new InArgument<List<int>>(),
                Current = new InArgument<int>()
            };

            forEachLoopEntity.EntityManager = entityManager;
            forEachLoopEntity.ResolveDependencies();

            var actorComponent = Substitute.For<ActorComponent>();
            var nestedLoopEntity = Substitute.For<Entity, ILoop>();
            //Entity.IsEnabled and ILoop.IsEnabled are differnt. We need to typecast to ILoop and mock it
            //This is because GetNextComponentProcess checks IComponent.IsEnabled to decide if component should be processed.
            (nestedLoopEntity as ILoop).IsEnabled.Returns(true);         
        
            forEachLoopEntity.AddComponent(nestedLoopEntity);
            nestedLoopEntity.GetNextComponentToProcess().Returns(new List<IComponent>() { actorComponent });
            nestedLoopEntity.Components.Returns(new List<IComponent>() { actorComponent });

            var iterationResult = forEachLoopEntity.GetNextComponentToProcess().ToList();

            Assert.AreEqual(6, iterationResult.Count);
            Assert.AreEqual(nestedLoopEntity, iterationResult.ElementAt(0));
            Assert.AreEqual(actorComponent, iterationResult.ElementAt(1));
            Assert.AreEqual(nestedLoopEntity, iterationResult.ElementAt(2));
            Assert.AreEqual(nestedLoopEntity, iterationResult.ElementAt(3));
            Assert.AreEqual(actorComponent, iterationResult.ElementAt(4));
            Assert.AreEqual(nestedLoopEntity, iterationResult.ElementAt(5));

            nestedLoopEntity.Received(2).ResetComponent();
        }
    }
}
