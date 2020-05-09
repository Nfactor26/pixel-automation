using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Test.Helpers;
using System.Linq;

namespace Pixel.Automation.Core.Tests.Component
{
    public class EntityTests
    {

        [Test]
        public void VerifyThatEntityCanBeInitialized()
        {
            Entity entity = new Entity("EntityName", "EntityTag");

            Assert.AreEqual("EntityName", entity.Name);
            Assert.AreEqual("EntityTag", entity.Tag);
            Assert.IsNotNull(entity.Components);
            Assert.IsTrue(entity.Components.Count == 0);
        }

        [Test]
        public void VerifyThatComponentsCanBeAddedToEntity()
        {
            IEntityManager entityManager = Substitute.For<IEntityManager>();

            IComponent component = Substitute.For<IComponent>();
            component.EntityManager.Returns(entityManager);
         

            Entity entity = new Entity("EntityName", "EntityTag");
            entity.EntityManager = entityManager;

            entity.AddComponent(component);

            Assert.AreEqual(1, entity.Components.Count);
            Assert.AreEqual(entity, component.Parent);
            Assert.AreEqual(0, component.ProcessOrder);

            component.Received(1).ResolveDependencies();
            component.Received(1).ValidateComponent();
            entityManager.Received(1).RestoreParentChildRelation(component, Arg.Any<bool>());

        }

        [Test]
        public void VerifyThatComponentsCanBeDeleted()
        {
            IEntityManager entityManager = Substitute.For<IEntityManager>();

            IComponent component = Substitute.For<IComponent>();
            component.EntityManager.Returns(entityManager);
           
            Entity entity = new Entity();
            entity.EntityManager = entityManager;
            entity.AddComponent(component);

            Assert.AreEqual(1, entity.Components.Count);
        
            entity.RemoveComponent(component);
            Assert.AreEqual(0, entity.Components.Count);
            Assert.IsNull(component.Parent);
            Assert.IsNull(component.EntityManager);
        }

        /// <summary>
        /// A processor calls GetNextComponentToProcess on an entity which returns a sequence of components that can be processed.
        /// ActorComponets are one of those components that should be processed and should be returned by Entity.
        /// </summary>
        [Test]
        public void VerifyThatActorComponentAreAccountedForProcessing()
        {
            IEntityManager entityManager = Substitute.For<IEntityManager>();
            Entity rootEntity = new Entity();
            rootEntity.EntityManager = entityManager;

            MockActorComponent actorComponent = new MockActorComponent();
            actorComponent.EntityManager = entityManager;

            rootEntity.AddComponent(actorComponent);

            var componentsToBeProcessed = rootEntity.GetNextComponentToProcess().ToList();

            Assert.AreEqual(1, componentsToBeProcessed.Count);
            Assert.IsTrue(componentsToBeProcessed.Contains(actorComponent));
        }


        /// <summary>
        /// A processor calls GetNextComponentToProcess on an entity which returns a sequence of components that can be processed.
        /// AsyncActorComponets are one of those components that should be processed and should be returned by Entity.
        /// </summary>
        [Test]
        public void VerifyThatAsyncActorComponentAreAccountedForProcessing()
        {
            IEntityManager entityManager = Substitute.For<IEntityManager>();
            Entity rootEntity = new Entity();
            rootEntity.EntityManager = entityManager;

            MockAsyncActorComponent actorComponent = new MockAsyncActorComponent();
            actorComponent.EntityManager = entityManager;

            rootEntity.AddComponent(actorComponent);

            var componentsToBeProcessed = rootEntity.GetNextComponentToProcess().ToList();

            Assert.AreEqual(1, componentsToBeProcessed.Count);
            Assert.IsTrue(componentsToBeProcessed.Contains(actorComponent));
        }

        /// <summary>
        /// A processor calls GetNextComponentToProcess on an entity which returns a sequence of components that can be processed.
        /// EntityProcessors are one of those components that should be processed and should be returned by Entity.
        /// </summary>
        [Test]
        public void VerifyThatEntityProcessorsAreAccountedForProcessing()
        {
            IEntityManager entityManager = Substitute.For<IEntityManager>();
            Entity rootEntity = new Entity();
            rootEntity.EntityManager = entityManager;

            IEntityProcessor entityProcessor = Substitute.For<IEntityProcessor>();
            entityProcessor.EntityManager.Returns(entityManager);
            entityProcessor.IsEnabled.Returns(true);

            rootEntity.AddComponent(entityProcessor);

            var componentsToBeProcessed = rootEntity.GetNextComponentToProcess().ToList();


            Assert.AreEqual(1, componentsToBeProcessed.Count);
            Assert.IsTrue(componentsToBeProcessed.Contains(entityProcessor));
        }

        /// <summary>
        /// A processor calls GetNextComponentToProcess on an entity which returns a sequence of components that can be processed.
        /// If a component is disabled, it should not be accounted for processing
        /// </summary>
        [Test]
        public void VerifyThatDisabledComponentAreNotAccountedForProcessing()
        {
            IEntityManager entityManager = Substitute.For<IEntityManager>();
            Entity rootEntity = new Entity();
            rootEntity.EntityManager = entityManager;

            MockActorComponent actorComponent = new MockActorComponent();
            actorComponent.EntityManager = entityManager;
            actorComponent.IsEnabled = false;

            rootEntity.AddComponent(actorComponent);

            var componentsToBeProcessed = rootEntity.GetNextComponentToProcess().ToList();

            Assert.AreEqual(0, componentsToBeProcessed.Count);         
        }


        /// <summary>
        /// A processor calls GetNextComponentToProcess on an entity which returns a sequence of components that can be processed.
        /// EntityProcessors are one of those components that should be processed and should be returned by Entity.
        /// </summary>
        [Test]
        public void VerifyThatNestedEntitiesAreAccountedForProcessing()
        {
            IEntityManager entityManager = Substitute.For<IEntityManager>();
            Entity rootEntity = new Entity();
            rootEntity.EntityManager = entityManager;

            Entity childEntity = new Entity();
            childEntity.EntityManager = entityManager;

            MockActorComponent actorComponent = new MockActorComponent();
            actorComponent.EntityManager = entityManager;

            MockAsyncActorComponent asyncActorComponent = new MockAsyncActorComponent();
            asyncActorComponent.EntityManager = entityManager;

            IEntityProcessor entityProcessor = Substitute.For<IEntityProcessor>();
            entityProcessor.EntityManager.Returns(entityManager);
            entityProcessor.IsEnabled.Returns(true);

            rootEntity.AddComponent(childEntity);
            rootEntity.AddComponent(asyncActorComponent);
       
            childEntity.AddComponent(actorComponent);
            childEntity.AddComponent(entityProcessor);

            var componentsToBeProcessed = rootEntity.GetNextComponentToProcess().ToList();


            Assert.AreEqual(5, componentsToBeProcessed.Count);
            Assert.IsTrue(componentsToBeProcessed.Contains(childEntity)); // Entities are returned twice - when encounterd and when after it's child are processed.
            Assert.IsTrue(componentsToBeProcessed.Contains(asyncActorComponent));
            Assert.IsTrue(componentsToBeProcessed.Contains(actorComponent));
            Assert.IsTrue(componentsToBeProcessed.Contains(entityProcessor));
        }
    }
}
