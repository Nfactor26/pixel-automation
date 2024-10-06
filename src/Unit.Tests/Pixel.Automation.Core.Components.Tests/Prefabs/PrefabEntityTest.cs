using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Test.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Tests
{
    public class PrefabEntityTest
    {
        /// <summary>
        /// Validate that PrefabEntity can load configured PrefabProcess and when processor will call GetNextComponentToProcess, it can return one component at a time
        /// from PrefabProcess in the order they are supposed to be processed.
        /// </summary>
        [Test]
        public async Task ValidateThatPrefabLoaderCanLoadPrefabProcess()
        {
            var prefabEntityManager = Substitute.For<IEntityManager>();
            prefabEntityManager.Arguments.Returns(new Person());
            var prefabProcessRootEntity = new Entity() { EntityManager = prefabEntityManager };
            var actorComponent = Substitute.For<ActorComponent>();
            prefabProcessRootEntity.AddComponent(actorComponent);

            var entityManager = Substitute.For<IEntityManager>();
            //setting argument of same type for prefab entity manager and entity manager. In a real proces, they will be usually different.
            entityManager.Arguments.Returns(new Person());
            var prefabLoader = Substitute.For<IPrefabLoader>();
            entityManager.GetServiceOfType<IPrefabLoader>().Returns(prefabLoader);
            prefabLoader.GetPrefabEntity(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEntityManager>()).Returns(prefabProcessRootEntity);
            prefabLoader.GetPrefabDataModelType(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEntityManager>()).Returns(typeof(Person));


            var inputMappingAction = new Action<object>((a) => { });
            var scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.CreateDelegateAsync<Action<object>>(Arg.Any<string>()).Returns(inputMappingAction);
            entityManager.GetScriptEngine().Returns(scriptEngine);

            var prefabEntity = new PrefabEntity()
            {
                EntityManager = entityManager,
                ApplicationId = "MockId",
                PrefabId = "PrefabId",
                InputMappingScriptFile = "InputMappingScript.csx",
                OutputMappingScriptFile = "OutputMappingScript.csx"
            };

            Assert.That(prefabEntity.Components.Count, Is.EqualTo(0));
            Assert.That(prefabEntity.EntityManager.Arguments.GetType(), Is.EqualTo(typeof(Person)));

            await prefabEntity.BeforeProcessAsync();
            await scriptEngine.Received(1).CreateDelegateAsync<Action<object>>(Arg.Is("InputMappingScript.csx"));
            Assert.That(await prefabEntity.GetPrefabDataModelType(), Is.EqualTo(typeof(Person)));

            Assert.That(prefabEntity.Components.Count, Is.EqualTo(1));

            var components = prefabEntity.GetNextComponentToProcess().ToList();
            Assert.That(components.Contains(prefabProcessRootEntity));
            Assert.That(components.Contains(actorComponent));

            await prefabEntity.OnCompletionAsync();
            await scriptEngine.Received(1).CreateDelegateAsync<Action<object>>(Arg.Is("OutputMappingScript.csx"));

            Assert.That(prefabEntity.Components.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// Valdiate that components can't be added to PrefabEntity by calling AddComponent() method on it 
        /// </summary>
        [Test]
        public void ValidateThatNoComponentsCanBeAddedToPrefabEntityUsingAddComponentMethod()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var prefabEntity = new PrefabEntity()
            {
                EntityManager = entityManager,
                ApplicationId = "MockId",
                PrefabId = "PrefabId",
                InputMappingScriptFile = "InputMappingScript.csx",
                OutputMappingScriptFile = "OutputMappingScript.csx"
            };

            var component = Substitute.For<IComponent>();
            prefabEntity.AddComponent(component);

            Assert.That(prefabEntity.Components.Count, Is.EqualTo(0));
        }
    }
}
