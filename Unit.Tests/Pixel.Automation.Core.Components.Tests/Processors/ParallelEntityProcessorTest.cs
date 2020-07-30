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
            
            processor.AddComponent(entityOne);
            entityOne.AddComponent(actorOne);

            processor.AddComponent(entityTwo);
            entityOne.AddComponent(actorTwo);

            await processor.BeginProcess();

            actorOne.Received(1).Act();
            actorOne.Received(1).BeforeProcess();
            actorOne.Received(1).OnCompletion();

            actorTwo.Received(1).Act();
            actorTwo.Received(1).BeforeProcess();
            actorTwo.Received(1).OnCompletion();

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
            actorOne.When(a => a.Act()).Do(a => { throw new ArgumentException(); });

            var entityTwo = Substitute.ForPartsOf<Entity>();
            var actorTwo = Substitute.For<ActorComponent>();
            actorTwo.When(a => a.Act()).Do(a => { throw new InvalidOperationException(); });


            ParallelEntityProcessor processor = new ParallelEntityProcessor();
            processor.EntityManager = entityManager;

            processor.AddComponent(entityOne);
            entityOne.AddComponent(actorOne);

            processor.AddComponent(entityTwo);
            entityOne.AddComponent(actorTwo);

            Assert.ThrowsAsync<AggregateException>(async () => { await processor.BeginProcess(); });

            await Task.CompletedTask;
        }
    }
}