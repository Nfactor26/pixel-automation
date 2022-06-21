using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Components.Processors;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Tests
{
    public class SequentialEntityProcessorTest
    {
        /// <summary>
        /// Validate that sequential processor can process its child components correctly
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task AsserThatSequentialProcessorCanProcessAllComponentsCorrectly()
        {         
            IEntityManager entityManager = Substitute.For<IEntityManager>();

            //Substituting for parts since we want to use GetNextComponentToProcess implementation
            var rootEntity = Substitute.ForPartsOf<Entity>(); 

            var actorComponent = Substitute.For<ActorComponent>();
         
            var asyncActorComponent = Substitute.For<ActorComponent>();         
     
            var disabledActorComponent = Substitute.For<ActorComponent>();
            disabledActorComponent.IsEnabled = false;
            

            var entityProcessor = Substitute.For<IEntityProcessor>();
            entityProcessor.IsEnabled = true;
           

            //Substituting for parts since we want to use GetNextComponentToProcess implementation
            var entity = Substitute.ForPartsOf<Entity>();
            var nestedActorComponent = Substitute.For<ActorComponent>();            
           

            SequentialEntityProcessor processor = new SequentialEntityProcessor();
            processor.EntityManager = entityManager;
            processor.AddComponent(rootEntity);
            rootEntity.AddComponent(actorComponent);
            rootEntity.AddComponent(asyncActorComponent);
            rootEntity.AddComponent(disabledActorComponent);
            rootEntity.AddComponent(entityProcessor);
            rootEntity.AddComponent(entity);
            entity.AddComponent(nestedActorComponent);
         
            await processor.BeginProcessAsync();

            await actorComponent.Received(1).ActAsync();
            await entity.Received(1).BeforeProcessAsync();
            await entity.Received(1).OnCompletionAsync();
          
            await asyncActorComponent.Received(1).ActAsync();         

            await disabledActorComponent.Received(0).ActAsync();
           
            await entityProcessor.Received(1).BeginProcessAsync();            
            
            await nestedActorComponent.Received(1).ActAsync();          

            await Task.CompletedTask;
        }

        /// <summary>
        /// Validate that Sequential processor throws exception if actor is faulted and configured not to continue on exception
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task AsserThatProcessorThrowsExceptionIfActorIsFaulted()
        {
            IEntityManager entityManager = Substitute.For<IEntityManager>();

            //Substituting for parts since we want to use GetNextComponentToProcess implementation
            var rootEntity = Substitute.ForPartsOf<Entity>();

            var actorComponent = Substitute.For<ActorComponent>();
            actorComponent.ContinueOnError = false;
            actorComponent.When(a => a.ActAsync()).Do(a => { throw new Exception(); });

         

            SequentialEntityProcessor processor = new SequentialEntityProcessor();
            processor.EntityManager = entityManager;
            processor.AddComponent(rootEntity);
            rootEntity.AddComponent(actorComponent);

            Assert.ThrowsAsync<Exception>(async () => { await processor.BeginProcessAsync(); });

            await actorComponent.Received(1).ActAsync();           

            await Task.CompletedTask;
        }

        /// <summary>
        /// Validate that Sequential processor doesn't throw exception if actor is faulted and configured to continue on exception
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task AsserThatProcessorContinuesExecutionIfActorIsConfiguredToContinueOnError()
        {
            IEntityManager entityManager = Substitute.For<IEntityManager>();

            //Substituting for parts since we want to use GetNextComponentToProcess implementation
            var rootEntity = Substitute.ForPartsOf<Entity>();

            var actorComponent = Substitute.For<ActorComponent>();
            actorComponent.ContinueOnError = true;
            actorComponent.When(a => a.ActAsync()).Do(a => { throw new Exception(); });

            var asyncActorComponent = Substitute.For<ActorComponent>();

            SequentialEntityProcessor processor = new SequentialEntityProcessor();
            processor.EntityManager = entityManager;
            processor.AddComponent(rootEntity);
            rootEntity.AddComponent(actorComponent);
            rootEntity.AddComponent(asyncActorComponent);

            await processor.BeginProcessAsync();

            await actorComponent.Received(1).ActAsync();
            await asyncActorComponent.Received(1).ActAsync();

            await Task.CompletedTask;
        }

        /// <summary>
        /// Validate that Sequential processor throws exception if async actor is faulted and configured not to continue on exception
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task AsserThatProcessorThrowsExceptionIfAsyncActorIsFaulted()
        {
            IEntityManager entityManager = Substitute.For<IEntityManager>();

            //Substituting for parts since we want to use GetNextComponentToProcess implementation
            var rootEntity = Substitute.ForPartsOf<Entity>();

            var asyncActorComponent = Substitute.For<ActorComponent>();
            asyncActorComponent.ContinueOnError = false;
            asyncActorComponent.When(a => a.ActAsync()).Do(a => { throw new Exception(); });



            SequentialEntityProcessor processor = new SequentialEntityProcessor();
            processor.EntityManager = entityManager;
            processor.AddComponent(rootEntity);
            rootEntity.AddComponent(asyncActorComponent);

            Assert.ThrowsAsync<Exception>(async () => { await processor.BeginProcessAsync(); });

            await asyncActorComponent.Received(1).ActAsync();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Validate that Sequential processor doesn't throw exception if async actor is faulted and configured to continue on exception
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task AsserThatProcessorContinuesExecutionIfAsyncActorIsConfiguredToContinueOnError()
        {
            IEntityManager entityManager = Substitute.For<IEntityManager>();

            //Substituting for parts since we want to use GetNextComponentToProcess implementation
            var rootEntity = Substitute.ForPartsOf<Entity>();

            var asyncActorComponent = Substitute.For<ActorComponent>();
            asyncActorComponent.ContinueOnError = true;
            asyncActorComponent.When(a => a.ActAsync()).Do(a => { throw new Exception(); });

            var actorComponent = Substitute.For<ActorComponent>();

            SequentialEntityProcessor processor = new SequentialEntityProcessor();
            processor.EntityManager = entityManager;
            processor.AddComponent(rootEntity);          
            rootEntity.AddComponent(asyncActorComponent);
            rootEntity.AddComponent(actorComponent);

            await processor.BeginProcessAsync();

            await asyncActorComponent.Received(1).ActAsync();
            await actorComponent.Received(1).ActAsync();
            await Task.CompletedTask;
        }
    }
}
