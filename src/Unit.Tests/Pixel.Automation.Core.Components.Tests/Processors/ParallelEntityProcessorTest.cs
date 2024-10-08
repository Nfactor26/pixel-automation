﻿using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Components.Processors;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Tests
{
    public class ParallelEntityProcessorTest
    {
        /// <summary>
        /// Validate that Parallel processor can process its child entities 
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task AsserThatParallelProcessorCanProcessAllComponentsCorrectly()
        {
            IEntityManager entityManager = Substitute.For<IEntityManager>();

            //Substituting for parts since we want to use GetNextComponentToProcess implementation
            var entityOne = Substitute.ForPartsOf<Entity>();
            var actorOne = Substitute.For<ActorComponent>();

            var entityTwo = Substitute.ForPartsOf<Entity>();
            var actorTwo = Substitute.For<ActorComponent>();
        

            ParallelEntityProcessor processor = new ParallelEntityProcessor();
            processor.EntityManager = entityManager;
            processor.ResolveDependencies();


            Assert.That(processor.Components.Count, Is.EqualTo(2));

            var parallelBlockOne = processor.Components[0] as Entity;
            parallelBlockOne.AddComponent(entityOne);
            entityOne.AddComponent(actorOne);

            var parallelBlockTwo = processor.Components[1] as Entity;
            parallelBlockTwo.AddComponent(entityTwo);
            entityTwo.AddComponent(actorTwo);

            await processor.BeginProcessAsync();

            await actorOne.Received(1).ActAsync();          
            await actorTwo.Received(1).ActAsync();
            await entityOne.Received(1).BeforeProcessAsync();
            await entityOne.Received(1).OnCompletionAsync();
            await entityTwo.Received(1).BeforeProcessAsync();
            await entityTwo.Received(1).OnCompletionAsync();

            await Task.CompletedTask;
        }

        /// <summary>
        /// Validate that Parallel processor throws an aggregate exception if there is exception while processing child entities parallely
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task AsserThatParallelProcessorThrowsAggregateExceptionOnFault()
        {
            IEntityManager entityManager = Substitute.For<IEntityManager>();

            //Substituting for parts since we want to use GetNextComponentToProcess implementation
            var entityOne = Substitute.ForPartsOf<Entity>();
            var actorOne = Substitute.For<ActorComponent>();
            actorOne.When(a => a.ActAsync()).Do(a => { throw new ArgumentException(); });
          
            ParallelEntityProcessor processor = new ParallelEntityProcessor();
            processor.EntityManager = entityManager;
            processor.ResolveDependencies();
            
            Assert.That(processor.Components.Count, Is.EqualTo(2));

            var parallelBlockOne = processor.Components[0] as Entity;
            parallelBlockOne.AddComponent(entityOne);
            entityOne.AddComponent(actorOne);          

            Assert.ThrowsAsync<AggregateException>(async () => { await processor.BeginProcessAsync(); });
            await entityOne.Received(1).BeforeProcessAsync();
            await entityOne.Received(1).OnFaultAsync(actorOne);

            await Task.CompletedTask;
        }

        [Test]
        public void AssertThatComponentsCanNotBeAddedToParallelProcessor()
        {
            ParallelEntityProcessor processor = new ParallelEntityProcessor();
            Assert.That(processor.Components.Count, Is.EqualTo(0), "There should be no components in a ParallelEntityProcessor by default");

            processor.AddComponent(new Entity());
            Assert.That(processor.Components.Count, Is.EqualTo(0), "ParallelEntityProcessor should not allow components to be added externally");
        }
    }
}
