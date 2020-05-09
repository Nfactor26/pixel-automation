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
    }
}
