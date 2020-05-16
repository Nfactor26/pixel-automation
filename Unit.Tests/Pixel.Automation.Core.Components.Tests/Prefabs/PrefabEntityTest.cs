using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Test.Helpers;
using System;
using System.Linq;

namespace Pixel.Automation.Core.Components.Tests
{
    public class PrefabEntityTest
    {
        /// <summary>
        /// Validate that PrefabEntity can load configured PrefabProcess and when processor will call GetNextComponentToProcess, it can return one component at a time
        /// from PrefabProcess in the order they are supposed to be processed.
        /// </summary>
        [Test]
        public void ValidateThatPrefabEntityCanLoadPrefabProcess()
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
            prefabLoader.LoadPrefab(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<PrefabVersion>(), Arg.Any<IEntityManager>()).Returns(prefabProcessRootEntity);

            var inputMappingAction = new Action<object>((a) => { });
            var scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.CreateDelegateAsync<Action<object>>(Arg.Any<string>()).Returns(inputMappingAction);
            entityManager.GetScriptEngine().Returns(scriptEngine);

            var prefabEntity = new PrefabEntity()
            {
                EntityManager = entityManager , 
                ApplicationId = "MockId", 
                PrefabId = "PrefabId",
                PrefabVersion = new PrefabVersion(),
                InputMappingScript = "InputMappingScript.csx", 
                OutputMappingScript = "OutputMappingScript.csx"
            };

            Assert.AreEqual(0, prefabEntity.Components.Count);
            Assert.AreEqual(typeof(Person), prefabEntity.DataModelType);

            prefabEntity.BeforeProcess();
            scriptEngine.Received(1).CreateDelegateAsync<Action<object>>(Arg.Is("InputMappingScript.csx"));
            Assert.AreEqual(typeof(Person), prefabEntity.PrefabDataModelType);

            Assert.AreEqual(1, prefabEntity.Components.Count);

            var components = prefabEntity.GetNextComponentToProcess().ToList();
            Assert.IsTrue(components.Contains(prefabProcessRootEntity));
            Assert.IsTrue(components.Contains(actorComponent));

            prefabEntity.OnCompletion();
            scriptEngine.Received(1).CreateDelegateAsync<Action<object>>(Arg.Is("OutputMappingScript.csx"));

            Assert.AreEqual(0, prefabEntity.Components.Count);
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
                PrefabVersion = new PrefabVersion(),
                InputMappingScript = "InputMappingScript.csx",
                OutputMappingScript = "OutputMappingScript.csx"
            };

            var component = Substitute.For<IComponent>();
            prefabEntity.AddComponent(component);

            Assert.AreEqual(0, prefabEntity.Components.Count);
        }
    }
}
