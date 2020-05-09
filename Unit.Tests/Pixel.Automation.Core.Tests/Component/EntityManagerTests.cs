using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Automation.Core.Tests.Component
{
    public class EntityManagerTests
    {
        private IServiceResolver serviceResolver;
        private IFileSystem fileSystem;
        private IArgumentProcessor argumentProcessor;
        private IScriptEngine scriptEngine;
        private ISerializer serializer;
        private IDevice syntheticMouse;
        private IDevice syntheticKeyboard;
        private Person dataModel;
        private EntityManager entityManager;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            this.serviceResolver = Substitute.For<IServiceResolver>();

            this.fileSystem = Substitute.For<IFileSystem>();
            this.argumentProcessor = Substitute.For<IArgumentProcessor>();
            this.serializer = Substitute.For<ISerializer>();

            this.scriptEngine = Substitute.For<IScriptEngine>();
            var propertyCollection = new List<PropertyDescription>();
            propertyCollection.Add(new PropertyDescription("PropertyOne", typeof(string)));
            propertyCollection.Add(new PropertyDescription("PropertyTwo", typeof(int)));
            this.scriptEngine.GetScriptVariables().Returns(
                propertyCollection
             );

            this.syntheticMouse = Substitute.For<ISyntheticMouse>();
            this.syntheticKeyboard = Substitute.For<ISyntheticKeyboard>();

            serviceResolver.Get<IArgumentProcessor>().Returns(this.argumentProcessor);
            serviceResolver.Get<IFileSystem>().Returns(this.fileSystem);
            serviceResolver.Get<ISerializer>().Returns(this.serializer);
            serviceResolver.Get<IScriptEngine>().Returns(this.scriptEngine);
            serviceResolver.Get<ISyntheticKeyboard>().Returns(this.syntheticKeyboard as ISyntheticKeyboard);
            serviceResolver.Get<ISyntheticMouse>().Returns(this.syntheticMouse as ISyntheticMouse);
            serviceResolver.GetAll<IDevice>().Returns(new List<IDevice>() { this.syntheticKeyboard, this.syntheticMouse });
            serviceResolver.When(x => x.RegisterDefault<EntityManager>(Arg.Any<EntityManager>())).Do((a) => { });
            serviceResolver.When(d => d.Dispose()).Do(x => { });

            this.dataModel = new Person();

            this.entityManager = new EntityManager(serviceResolver);
            this.entityManager.Arguments = this.dataModel;


        }

        /// <summary>
        /// Verify that EntityManager can be initialized and it registers itself as default instance of EntityManager with the service resolver
        /// </summary>
        [Test]
        [Order(10)]
        public void VerifyThatEntityManagerCanBeInitialized()
        {

            this.serviceResolver.Received(1).RegisterDefault<EntityManager>(entityManager);
        }


        /// <summary>
        /// Verify that entity manager can provide requested services
        /// </summary>
        [Test]
        [Order(20)]
        public void VerifyThatServicesCanBeRetrievedUsingGetServiceOfTypeMethod()
        {
            Assert.AreSame(this.serializer, entityManager.GetServiceOfType<ISerializer>());
            Assert.AreSame(this.fileSystem, entityManager.GetServiceOfType<IFileSystem>());
            Assert.AreSame(this.scriptEngine, entityManager.GetServiceOfType<IScriptEngine>());
            Assert.AreSame(this.argumentProcessor, entityManager.GetServiceOfType<IArgumentProcessor>());
            Assert.AreSame(this.syntheticMouse, entityManager.GetServiceOfType<ISyntheticMouse>());
            Assert.AreSame(this.syntheticKeyboard, entityManager.GetServiceOfType<ISyntheticKeyboard>());

            this.serviceResolver.Received(1).Get<ISerializer>();
            this.serviceResolver.Received(1).Get<IFileSystem>();
            this.serviceResolver.Received(1).Get<IScriptEngine>();
            this.serviceResolver.Received(1).Get<IArgumentProcessor>();
            this.serviceResolver.Received(1).Get<ISyntheticMouse>();
            this.serviceResolver.Received(1).Get<ISyntheticKeyboard>();
        }

        [Test]
        [Order(30)]
        public void VerifyThatArgumentProcessorCanBeRetrieved()
        {
            var argumentProcessor = entityManager.GetArgumentProcessor();
            Assert.AreSame(this.argumentProcessor, argumentProcessor);
        }


        [Test]
        [Order(40)]
        public void VerifyThatScriptEngineCanBeRetrieved()
        {
            var scriptEngine = entityManager.GetScriptEngine();
            Assert.AreSame(this.scriptEngine, scriptEngine);
        }

        [Test]
        [Order(50)]
        public void VerifyThatAllServicesOfAGivenTypeCanBeRetrievedUsingGetAllServiceOfType()
        {
            var devices = entityManager.GetAllServicesOfType<IDevice>();

            Assert.IsTrue(devices.Contains(this.syntheticKeyboard));
            Assert.IsTrue(devices.Contains(this.syntheticMouse));

            this.serviceResolver.Received(1).GetAll<IDevice>();
        }

        [Test]
        [Order(60)]
        public void VerifyThatDataMembersAndScriptVariablesOfGivenTypeCanBeRetrieved()
        {
            var stringVariables = entityManager.GetPropertiesOfType(typeof(string));
            var intVariables = entityManager.GetPropertiesOfType(typeof(int));
            var addresssVariables = entityManager.GetPropertiesOfType(typeof(Address));
            var personCollectionVariables = entityManager.GetPropertiesOfType(typeof(List<Person>));

            Assert.AreEqual(2, stringVariables.Count());
            Assert.IsTrue(stringVariables.Contains("PropertyOne"));
            Assert.IsTrue(stringVariables.Contains("Name"));

            Assert.AreEqual(2, intVariables.Count());
            Assert.IsTrue(intVariables.Contains("PropertyTwo"));
            Assert.IsTrue(intVariables.Contains("Age"));


            Assert.AreEqual(1, addresssVariables.Count());
            Assert.IsTrue(addresssVariables.Contains("Address"));

            Assert.AreEqual(1, personCollectionVariables.Count());
            Assert.IsTrue(personCollectionVariables.Contains("Friends"));

        }

        [Test]
        [Order(70)]
        public void VerifyThatEntityManagerCanBeDisposed()
        {
            Entity rootEntity = new Entity("Root", "Root");
            rootEntity.EntityManager = this.entityManager;
            var disposableComponent = Substitute.For<IComponent, IDisposable>();
            disposableComponent.When(d => (d as IDisposable).Dispose()).Do(x => { });
            rootEntity.AddComponent(disposableComponent);
            entityManager.RootEntity = rootEntity;

            entityManager.Dispose();

            (disposableComponent as IDisposable).Received(1).Dispose();
            serviceResolver.Received(1).Dispose();
            Assert.IsNull(entityManager.RootEntity);
            Assert.IsNull(entityManager.Arguments);
        }

    }

}
