﻿using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Components.Loops;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Automation.Core.Components.Tests
{
    public class ForLoopEntityTest
    {
        /// <summary>
        /// Vadidate that For loop entity runs for exactly configured number of times
        /// </summary>
        [Test]
        public void ValidateThatForLoopEntityCanProcessConfiguredIterationsCorrectly()
        {
            var entityManager = Substitute.For<IEntityManager>();
      
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ScriptsDirectory.Returns(Environment.CurrentDirectory);
            fileSystem.ReadAllText(Arg.Any<string>()).Returns("int i = 0 ;i < 3; i++");

            var scriptEngine = Substitute.For<IScriptEngine>();         
            scriptEngine.ExecuteScriptAsync(Arg.Is<string>("i < 3")).Returns(new ScriptResult(true), new ScriptResult(true), new ScriptResult(false));
            

            entityManager.GetCurrentFileSystem().Returns(fileSystem);
            entityManager.GetScriptEngine().Returns(scriptEngine);

            var forLoopEntity = new ForLoopEntity() { ScriptFile = Guid.NewGuid().ToString() };
            forLoopEntity.EntityManager = entityManager;
            forLoopEntity.ResolveDependencies();
            var placeHolderEntity = forLoopEntity.Components[0] as PlaceHolderEntity;
            Assert.That(placeHolderEntity is not null);

            var actorComponent = Substitute.For<ActorComponent>();
            placeHolderEntity.AddComponent(actorComponent);

            var iterationResult = forLoopEntity.GetNextComponentToProcess().ToList();

            Assert.That(iterationResult.Count, Is.EqualTo(2));
            Assert.That(iterationResult.ElementAt(0), Is.EqualTo(actorComponent));
            Assert.That(iterationResult.ElementAt(1), Is.EqualTo(actorComponent));

            scriptEngine.Received(6).ExecuteScriptAsync(Arg.Any<string>());
        }

        /// <summary>
        /// Vadidate that For loop entity can work with other nested loops
        /// </summary>
        [Test]
        public void ValidateThatForLoopEntityCanWorkWithNestedLoops()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ScriptsDirectory.Returns(Environment.CurrentDirectory);
            fileSystem.ReadAllText(Arg.Any<string>()).Returns("int i = 0 ;i < 3; i++");

            var scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.ExecuteScriptAsync(Arg.Is<string>("i < 3")).Returns(new ScriptResult(true), new ScriptResult(true), new ScriptResult(false));


            entityManager.GetCurrentFileSystem().Returns(fileSystem);
            entityManager.GetScriptEngine().Returns(scriptEngine);

            var forLoopEntity = new ForLoopEntity() { ScriptFile = Guid.NewGuid().ToString() };
            forLoopEntity.EntityManager = entityManager;
            forLoopEntity.ResolveDependencies();
            var placeHolderEntity = forLoopEntity.Components[0] as PlaceHolderEntity;
            Assert.That(placeHolderEntity is not null);

            var actorComponent = Substitute.For<ActorComponent>();
            var nestedLoopEntity = Substitute.For<Entity, ILoop>();
            //Entity.IsEnabled and ILoop.IsEnabled are differnt. We need to typecast to ILoop and mock it
            //This is because GetNextComponentProcess checks IComponent.IsEnabled to decide if component should be processed.
            (nestedLoopEntity as ILoop).IsEnabled.Returns(true);

            placeHolderEntity.AddComponent(nestedLoopEntity);
            nestedLoopEntity.GetNextComponentToProcess().Returns(new List<IComponent>() { actorComponent });           
            nestedLoopEntity.Components.Returns(new List<IComponent>() { actorComponent });
       

            var iterationResult = forLoopEntity.GetNextComponentToProcess().ToList();

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
