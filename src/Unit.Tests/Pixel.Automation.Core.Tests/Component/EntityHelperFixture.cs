using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Test.Helpers;
using System;
using System.Linq;

namespace Pixel.Automation.Core.Tests
{
    public class EntityHelperFixture
    {
        private  Entity rootEntity;

        [OneTimeSetUp]
        public void Setup()
        {
            rootEntity = ProcessFactory.CreateMockProcess();
        }

        [Test]
        public void ValidateThatAllComponentsCanBeRetrieived()
        {
            var foundComponents = rootEntity.GetAllComponents();

            Assert.That(foundComponents.Count, Is.EqualTo(14));
        }

        [Test]
        public void ValidateThatComponentsCanBeRetrieivedByTypeForDescendantsScope()
        {
            var foundComponents = rootEntity.GetComponentsOfType<MockAsyncActorComponent>(Enums.SearchScope.Descendants);

            Assert.That(foundComponents.Count(), Is.EqualTo(2));
        }

        [Test]
        public void ValidateThatComponentsCanBeRetrieivedByTypeForChildScope()
        {
            var foundComponents = rootEntity.GetComponentsOfType<MockAsyncActorComponent>(Enums.SearchScope.Children);      

            Assert.That(foundComponents.Count(), Is.EqualTo(0));
        }

        [Test]
        public void ValidateThatArgumentExceptionIsThrownWhenTryingToRetrieveByTypeForUnsuppoertedScope()
        {
           Assert.Throws<ArgumentException>(() => { rootEntity.GetComponentsOfType<MockAsyncActorComponent>(Enums.SearchScope.Ancestor); }) ;           
        }


        [Test]
        public void ValidateThatFirstComponentCanBeRetrieivedByTypeForDescendantsScope()
        {
            var component = rootEntity.GetFirstComponentOfType<MockAsyncActorComponent>(Enums.SearchScope.Descendants);

            Assert.That(component is not null);
        }

        [Test]
        public void ValidateThatFirstComponentCanBeRetrieivedByTypeForChildScope()
        {
            //processor component has two child actor componentts named AEight and ANine
            var processor = rootEntity.GetFirstComponentOfType<MockEntityProcessor>(Enums.SearchScope.Descendants);
            var actor = processor.GetFirstComponentOfType<MockActorComponent>(Enums.SearchScope.Children);

            Assert.That(actor is not null);
            Assert.That(actor.Name, Is.EqualTo("E3-P1-A1"));
        }

        [Test]
        public void ValidateThatArgumentExceptionIsThrownWhenTryingToRetrieveFirstOfTypeForUnsuppoertedScope()
        {
            Assert.Throws<ArgumentException>(() => { rootEntity.GetFirstComponentOfType<MockAsyncActorComponent>(Enums.SearchScope.Ancestor); });
        }


        [Test]
        public void ValidateThatExceptionIsThrownWhenFirstComponentOfTypeCanNotBeLocated()
        {
            //processor component has two child actor componentts named AEight and ANine
            var entity = rootEntity.GetFirstComponentOfType<MockWhileLoopEntity>(Enums.SearchScope.Descendants);
          
            Assert.Throws<MissingComponentException>(()=> { entity.GetFirstComponentOfType<MockAsyncActorComponent>(Enums.SearchScope.Descendants, true); });
        
        }

        [Test]
        public void ValidateThatComponentsCanBeRetrieivedByTagForDescendantsScope()
        {
            var foundComponents = rootEntity.GetComponentsByTag("Actor", Enums.SearchScope.Descendants);

            Assert.That(foundComponents.Count(), Is.EqualTo(7));
        }

        [Test]
        public void ValidateThatComponentsCanBeRetrieivedByTagForChildScope()
        {
            var foundComponents = rootEntity.GetComponentsByTag("Actor", Enums.SearchScope.Children);

            Assert.That(foundComponents.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ValidateThatArgumentExceptionIsThrownWhenTryingToRetrieveByTagForUnsuppoertedScope()
        {
            Assert.Throws<ArgumentException>(() => { rootEntity.GetComponentsByTag("Actor", Enums.SearchScope.Ancestor); });
        }


        [Test]
        public void ValidateThatComponentsCanBeRetrieivedBIdForDescendantsScope()
        {
            var whileLoopEntity = rootEntity.GetFirstComponentOfType<MockWhileLoopEntity>(Enums.SearchScope.Descendants);
            var foundEntity = rootEntity.GetComponentById(whileLoopEntity.Id, Enums.SearchScope.Descendants);

            Assert.That(whileLoopEntity, Is.SameAs(foundEntity));
        }

        [Test]
        public void ValidateThatComponentsCanBeRetrieivedBIdForChildScope()
        {
            var actorComponent = rootEntity.GetFirstComponentOfType<MockActorComponent>(Enums.SearchScope.Children);
            var foundEntity = rootEntity.GetComponentById(actorComponent.Id, Enums.SearchScope.Children);

            Assert.That(actorComponent, Is.SameAs(foundEntity));
        }

        [Test]
        public void ValidateThatArgumentExceptionIsThrownWhenTryingToRetrieveByIdForUnsuppoertedScope()
        {
            Assert.Throws<ArgumentException>(() => { rootEntity.GetComponentById("Id", Enums.SearchScope.Ancestor); });
        }


        [Test]
        public void ValidateThatComponentsCanBeRetrieivedByNameForDescendantsScope()
        {
            var foundComponents = rootEntity.GetComponentsByName("E1-E1-L1-A1", Enums.SearchScope.Descendants);

            Assert.That(foundComponents.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ValidateThatComponentsCanBeRetrieivedByNameForChildScope()
        {
            var foundComponents = rootEntity.GetComponentsByName("E3", Enums.SearchScope.Children);

            Assert.That(foundComponents.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ValidateThatArgumentExceptionIsThrownWhenTryingToRetrieveByNameForUnsuppoertedScope()
        {
            Assert.Throws<ArgumentException>(() => { rootEntity.GetComponentsByName("E3", Enums.SearchScope.Ancestor); });
        }


        [Test]
        public void ValidateThatComponentsCanBeRetrieivedByAttributeForDescendantsScope()
        {
            var foundComponents = rootEntity.GetComponentsWithAttribute<BuilderAttribute>(Enums.SearchScope.Descendants);

            Assert.That(foundComponents.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ValidateThatComponentsCanBeRetrieivedByAttributeForChildScope()
        {
            var entityComponent = rootEntity.GetComponentsByName("E1-E1", Enums.SearchScope.Descendants).Single() as Entity;
            var foundComponents = entityComponent.GetComponentsWithAttribute<BuilderAttribute>(Enums.SearchScope.Children);

            Assert.That(foundComponents.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ValidateThatArgumentExceptionIsThrownWhenTryingToRetrieveByAttributeForUnsuppoertedScope()
        {
            Assert.Throws<ArgumentException>(() => { rootEntity.GetComponentsWithAttribute<BuilderAttribute>(Enums.SearchScope.Ancestor); });
        }

        [Test]
        public void ValidateThatRootEntityCanBeRetrievied()
        {
            var childComponent = rootEntity.GetFirstComponentOfType<MockWhileLoopEntity>(Enums.SearchScope.Descendants);

            var rootComponent = childComponent.GetRootEntity();

            Assert.That(rootComponent, Is.SameAs(rootEntity));
        }


        [Test]
        public void ValidateThatResetHierarchyResetsAllComponents()
        {
            IEntityManager entityManager = Substitute.For<IEntityManager>();
          
            Entity rootEntity = Substitute.ForPartsOf<Entity>();
            rootEntity.EntityManager = entityManager;
            rootEntity.When(x => x.ResetComponent()).Do(x => { });
         
            Entity childEntity = Substitute.ForPartsOf<Entity>();     
            childEntity.When(x => x.ResetComponent()).Do(x => { });

            IComponent componentOne = Substitute.For<IComponent>();
            componentOne.When(x => x.ResetComponent()).Do(x => { });
            rootEntity.AddComponent(componentOne);
            rootEntity.AddComponent(childEntity);
          
            IComponent componentTwo = Substitute.For<IComponent>();
            componentTwo.When(x => x.ResetComponent()).Do(x => { });
            childEntity.AddComponent(componentTwo);


            rootEntity.ResetHierarchy();

            rootEntity.Received(1).ResetComponent();
            childEntity.Received(1).ResetComponent();
            componentOne.Received(1).ResetComponent();
            componentTwo.Received(1).ResetComponent();
        }


        [Test]
        public void ValidateThatResetDescendantsResetsAllComponentsButSelf()
        {
            IEntityManager entityManager = Substitute.For<IEntityManager>();

            Entity rootEntity = Substitute.ForPartsOf<Entity>();
            rootEntity.EntityManager = entityManager;
            rootEntity.When(x => x.ResetComponent()).Do(x => { });

            Entity childEntity = Substitute.ForPartsOf<Entity>();
            childEntity.When(x => x.ResetComponent()).Do(x => { });

            IComponent componentOne = Substitute.For<IComponent>();
            componentOne.When(x => x.ResetComponent()).Do(x => { });
            rootEntity.AddComponent(componentOne);
            rootEntity.AddComponent(childEntity);

            IComponent componentTwo = Substitute.For<IComponent>();
            componentTwo.When(x => x.ResetComponent()).Do(x => { });
            childEntity.AddComponent(componentTwo);


            rootEntity.ResetDescendants();

            rootEntity.Received(0).ResetComponent();
            childEntity.Received(1).ResetComponent();
            componentOne.Received(1).ResetComponent();
            componentTwo.Received(1).ResetComponent();
        }

        /// <summary>
        /// Validate that ancestor component of a requested type can be retreived
        /// </summary>
        [Test]
        public void ValidateThatAncestorComponentOfGivenTypeCanBeRetrieved()
        {
            var actorComponent = rootEntity.GetComponentsByName("E3-P1-A1", Enums.SearchScope.Descendants).Single();
            var ancestorEntity = rootEntity.GetComponentsByName("E3-P1", Enums.SearchScope.Descendants).Single();

            var foundAncestorOfTypeEntity = actorComponent.GetAnsecstorOfType<Entity>();

            Assert.That(foundAncestorOfTypeEntity, Is.SameAs(ancestorEntity));
        }


        /// <summary>
        /// Validate that MissingComponentException is thrown if Ancestor component of requested type can't be located
        /// </summary>
        [Test]
        public void ValidateThatExcpetionIsRaisedWhenAncestorComponentOfGivenTypeCanNotBeRetrieved()
        {
            var actorComponent = rootEntity.GetComponentsByName("E3-P1-A1", Enums.SearchScope.Descendants).Single();
            Assert.Throws<MissingComponentException>(() => { actorComponent.GetAnsecstorOfType<ServiceComponent>(); }) ;          
        }

        /// <summary>
        /// Validate that TryGetAncestoryOfType can locate ancestor of requested type when present
        /// </summary>
        [Test]
        public void ValidateThatTryGetAncestorOfTypeCanLocateAncestorOfGivenType()
        {
            var actorComponent = rootEntity.GetComponentsByName("E3-P1-A1", Enums.SearchScope.Descendants).Single();
            var ancestorEntity = rootEntity.GetComponentsByName("E3-P1", Enums.SearchScope.Descendants).Single();
            
            bool found = actorComponent.TryGetAnsecstorOfType<Entity>(out Entity foundAncestorOfType);

            Assert.That(found);
            Assert.That(foundAncestorOfType, Is.SameAs(ancestorEntity));
        }

        /// <summary>
        /// Validate that TryGetAncestoryOfType does not throw exception if ancestor can not be located
        /// </summary>
        [Test]
        public void ValidateThatTryGetAncestorOfTypeDoesNotThrowExceptionIfAncestorCanNotBeLocated()
        {
            var actorComponent = rootEntity.GetComponentsByName("E3-P1-A1", Enums.SearchScope.Descendants).Single();

            bool found = actorComponent.TryGetAnsecstorOfType<ServiceComponent>(out ServiceComponent foundComponent);

            Assert.That(found == false);
            Assert.That(foundComponent is null);
        }
    }
}
