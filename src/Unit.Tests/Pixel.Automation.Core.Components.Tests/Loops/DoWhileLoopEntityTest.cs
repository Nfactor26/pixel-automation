using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Components.Loops;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Automation.Core.Components.Tests
{
    public class DoWhileLoopEntityTest
    {
        /// <summary>
        /// Vadidate that Do While loop entity runs until specified condition is true
        /// </summary>
        [Test]
        public void ValidateThatDoWhileLoopEntityLoopsUntilConditionStaysSatisfied()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.ExecuteFileAsync(Arg.Any<string>()).Returns(new ScriptResult(true), new ScriptResult(false));

            entityManager.GetScriptEngine().Returns(scriptEngine);

            var whileLoopEntity = new DoWhileLoopEntity() { ScriptFile = Guid.NewGuid().ToString() };
            whileLoopEntity.EntityManager = entityManager;
            whileLoopEntity.ResolveDependencies();
            var placeHolderEntity = whileLoopEntity.Components[0] as PlaceHolderEntity;
            Assert.IsNotNull(placeHolderEntity);

            var actorComponent = Substitute.For<ActorComponent>();
            placeHolderEntity.AddComponent(actorComponent);

            var iterationResult = whileLoopEntity.GetNextComponentToProcess().ToList();

            Assert.AreEqual(2, iterationResult.Count);
            Assert.AreEqual(actorComponent, iterationResult.ElementAt(0));
            Assert.AreEqual(actorComponent, iterationResult.ElementAt(1));

            scriptEngine.Received(2).ExecuteFileAsync(Arg.Any<string>());
        }

        /// <summary>
        /// Vadidate that Do while loop entity can work with other nested loops
        /// </summary>
        [Test]
        public void ValidateThatWhileLoopEntityCanWorkWithNestedLoops()
        {
            var entityManager = Substitute.For<IEntityManager>();


            var scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.ExecuteFileAsync(Arg.Any<string>()).Returns(new ScriptResult(true), new ScriptResult(false));

            entityManager.GetScriptEngine().Returns(scriptEngine);

            var whileLoopEntity = new DoWhileLoopEntity() { ScriptFile = Guid.NewGuid().ToString() };
            whileLoopEntity.EntityManager = entityManager;
            whileLoopEntity.ResolveDependencies();
            var placeHolderEntity = whileLoopEntity.Components[0] as PlaceHolderEntity;
            Assert.IsNotNull(placeHolderEntity);

            var actorComponent = Substitute.For<ActorComponent>();
            var nestedLoopEntity = Substitute.For<Entity, ILoop>();
            //Entity.IsEnabled and ILoop.IsEnabled are differnt. We need to typecast to ILoop and mock it
            //This is because GetNextComponentProcess checks IComponent.IsEnabled to decide if component should be processed.
            (nestedLoopEntity as ILoop).IsEnabled.Returns(true);

            placeHolderEntity.AddComponent(nestedLoopEntity);
            nestedLoopEntity.GetNextComponentToProcess().Returns(new List<IComponent>() { actorComponent });
            nestedLoopEntity.Components.Returns(new List<IComponent>() { actorComponent });


            var iterationResult = whileLoopEntity.GetNextComponentToProcess().ToList();

            Assert.AreEqual(6, iterationResult.Count);
            Assert.AreEqual(nestedLoopEntity, iterationResult.ElementAt(0));
            Assert.AreEqual(actorComponent, iterationResult.ElementAt(1));
            Assert.AreEqual(nestedLoopEntity, iterationResult.ElementAt(2));
            Assert.AreEqual(nestedLoopEntity, iterationResult.ElementAt(3));
            Assert.AreEqual(actorComponent, iterationResult.ElementAt(4));
            Assert.AreEqual(nestedLoopEntity, iterationResult.ElementAt(5));

            nestedLoopEntity.Received(1).ResetComponent();
        }
    }
}
