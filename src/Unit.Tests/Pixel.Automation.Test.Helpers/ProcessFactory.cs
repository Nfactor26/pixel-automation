using NSubstitute;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Test.Helpers
{
    public class ProcessFactory
    {
        public static Entity CreateMockProcess()
        {
            IEntityManager entityManager = Substitute.For<IEntityManager>();

            ///------------------------------------------------------------
            ///  RootEntity 
            ///    -    MockActor   (A1, Actor)
            ///    -    Entity      (E1, Entity)
            ///             -   MockActor   (E1-A1, Actor)
            ///             -   MockActor   (E1-A2, Actor)
            ///             -   Entity      (E1-E1, Entity)
            ///                     -   MockAsyncActor  (E1-E1-A1, AsyncActor)
            ///                     -   MockWhileLoopEntity  (E1-E1-L1, Loop)
            ///                             -   MockActorComponent (E1-E1-L1-A1, Actor)
            ///                             -   MockActorComponent (E1-E1-L1-A2, Actor)
            ///    -    Entity      (E3, Entity)
            ///             -   MockAsyncActor  (E3-A1, AsyncActor)  
            ///             -   MockEntityProcessor (E3-P1, Processor)
            ///                     -   MockActor (E3-P1-A1, Actor)
            ///                     -   MockActor (E3-P1-A2, Actor);
            ///------------------------------------------------------------


            Entity rootEntity = new Entity();
            rootEntity.EntityManager = entityManager;

            MockActorComponent componentOne = new MockActorComponent("A1", "Actor");
            rootEntity.AddComponent(componentOne);


            Entity entityOne = new Entity("E1", "Entity");
            rootEntity.AddComponent(entityOne);                      
        

            MockActorComponent componentTwo = new MockActorComponent("E1-A1", "Actor");
            entityOne.AddComponent(componentTwo);

            MockActorComponent componentThree = new MockActorComponent("E1-A2", "Actor");
            entityOne.AddComponent(componentThree);

            Entity entityTwo = new Entity("E1-E1", "Entity");
            entityOne.AddComponent(entityTwo);

            Entity entityThree = new Entity("E3", "Entity");
            rootEntity.AddComponent(entityThree);

            MockAsyncActorComponent componentFour = new MockAsyncActorComponent("E3-A1", "AsyncActor");
            entityThree.AddComponent(componentFour);

            MockAsyncActorComponent componentFive = new MockAsyncActorComponent("E1-E1-A1", "AsyncActor");
            entityTwo.AddComponent(componentFive);


            MockWhileLoopEntity entityFour = new MockWhileLoopEntity("E1-E1-L1", "Loop");
            entityTwo.AddComponent(entityFour);

            MockActorComponent componentSix = new MockActorComponent("E1-E1-L1-A1", "Actor");
            entityFour.AddComponent(componentSix);

            MockActorComponent componentSeven = new MockActorComponent("E1-E1-L1-A2", "Actor") { IsEnabled = false };
            entityFour.AddComponent(componentSeven);

            MockEntityProcessor entityProcessorOne = new MockEntityProcessor("E3-P1", "Processor");
            entityThree.AddComponent(entityProcessorOne);

            MockActorComponent componentEight = new MockActorComponent("E3-P1-A1", "Actor");
            entityProcessorOne.AddComponent(componentEight);

            MockActorComponent componentNine = new MockActorComponent("E3-P1-A2", "Actor");
            entityProcessorOne.AddComponent(componentNine);

            return rootEntity;
        }

        /// <summary>
        /// Create a mock processors that uses  NSubstitute mocks which can be asserts for calls received
        /// </summary>
        /// <returns></returns>
        public static Entity CreateMockProcessWithSubstitutes()
        {
            IEntityManager entityManager = Substitute.For<IEntityManager>();

            ///------------------------------------------------------------
            ///  RootEntity 
            ///    -    MockActor   (A1, Actor)
            ///    -    Entity      (E1, Entity)
            ///             -   MockActor   (E1-A1, Actor)
            ///             -   MockActor   (E1-A2, Actor)
            ///             -   Entity      (E1-E1, Entity)
            ///                     -   MockAsyncActor  (E1-E1-A1, AsyncActor)
            ///                     -   MockWhileLoopEntity  (E1-E1-L1, Loop)
            ///                             -   MockActorComponent (E1-E1-L1-A1, Actor)
            ///                             -   MockActorComponent (E1-E1-L1-A2, Actor)
            ///    -    Entity      (E3, Entity)
            ///             -   MockAsyncActor  (E3-A1, AsyncActor)  
            ///             -   MockEntityProcessor (E3-P1, Processor)
            ///                     -   MockActor (E3-P1-A1, Actor)
            ///                     -   MockActor (E3-P1-A2, Actor);
            ///------------------------------------------------------------


            Entity rootEntity = new Entity();
            rootEntity.EntityManager = entityManager;

            var componentOne = Substitute.For<ActorComponent>();
            componentOne.Name = "A1";
            componentOne.Tag = "Actor";
            rootEntity.AddComponent(componentOne);


            Entity entityOne = new Entity("E1", "Entity");
            rootEntity.AddComponent(entityOne);


            var componentTwo = Substitute.For<ActorComponent>();
            componentTwo.Name = "E1-A1";
            componentTwo.Tag = "Actor";
            entityOne.AddComponent(componentTwo);

            var componentThree = Substitute.For<ActorComponent>();
            componentThree.Name = "E1-A2";
            componentThree.Tag = "Actor";
            entityOne.AddComponent(componentThree);

            Entity entityTwo = new Entity("E1-E1", "Entity");
            entityOne.AddComponent(entityTwo);

            Entity entityThree = new Entity("E3", "Entity");
            rootEntity.AddComponent(entityThree);

            var componentFour = Substitute.For<ActorComponent>();
            componentFour.Name = "E3-A1";
            componentFour.Tag = "Actor";
            entityThree.AddComponent(componentFour);

            var componentFive = Substitute.For<ActorComponent>();
            componentFive.Name = "E1-E1-A1";
            componentFive.Tag = "Actor";
            entityTwo.AddComponent(componentFive);


            MockWhileLoopEntity entityFour = new MockWhileLoopEntity("E1-E1-L1", "Loop");
            entityTwo.AddComponent(entityFour);

            var componentSix = Substitute.For<ActorComponent>();
            componentFive.Name = "E1 - E1 - L1 - A1";
            componentFive.Tag = "Actor";
            entityFour.AddComponent(componentSix);

            var componentSeven = Substitute.For<ActorComponent>();
            componentFive.Name = "E1-E1-L1-A2";
            componentFive.Tag = "Actor";
            componentSeven.IsEnabled = false;
            entityFour.AddComponent(componentSeven);

            var entityProcessorOne = new MockEntityProcessor("E3-P1", "Processor");
            entityThree.AddComponent(entityProcessorOne);

            var componentEight = Substitute.For<ActorComponent>();
            componentFive.Name = "E3-P1-A1";
            componentFive.Tag = "Actor";
            entityProcessorOne.AddComponent(componentEight);

            var componentNine = Substitute.For<ActorComponent>();
            componentFive.Name = "E3-P1-A2";
            componentFive.Tag = "Actor";
            entityProcessorOne.AddComponent(componentNine);

            return rootEntity;
        }

    }
}
