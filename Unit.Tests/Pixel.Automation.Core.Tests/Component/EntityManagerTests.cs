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
        private IScriptEngineFactory scriptEngineFactory;
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

            this.scriptEngineFactory = Substitute.For<IScriptEngineFactory>();
            this.scriptEngineFactory.CreateScriptEngine(Arg.Any<string>()).Returns(this.scriptEngine);

            this.syntheticMouse = Substitute.For<ISyntheticMouse>();
            this.syntheticKeyboard = Substitute.For<ISyntheticKeyboard>();

            serviceResolver.Get<IArgumentProcessor>().Returns(this.argumentProcessor);
            serviceResolver.Get<IFileSystem>().Returns(this.fileSystem);
            serviceResolver.Get<ISerializer>().Returns(this.serializer);
            serviceResolver.Get<IScriptEngineFactory>().Returns(this.scriptEngineFactory);
            serviceResolver.Get<IScriptEngine>().Returns(this.scriptEngine);
            serviceResolver.Get<ISyntheticKeyboard>().Returns(this.syntheticKeyboard as ISyntheticKeyboard);
            serviceResolver.Get<ISyntheticMouse>().Returns(this.syntheticMouse as ISyntheticMouse);
            serviceResolver.GetAll<IDevice>().Returns(new List<IDevice>() { this.syntheticKeyboard, this.syntheticMouse });
            serviceResolver.When(x => x.RegisterDefault<EntityManager>(Arg.Any<EntityManager>())).Do((a) => { });
            serviceResolver.When(d => d.Dispose()).Do(x => { });

            this.dataModel = new Person();

            this.entityManager = new EntityManager(serviceResolver);
            this.entityManager.SetCurrentFileSystem(fileSystem);
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
            this.serviceResolver.Received(2).Get<IArgumentProcessor>();
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


        /// <summary>
        /// Validate that for a given component it's owner application details can be retrieved using GetOwnerApplication() from EntityManager
        /// when component is a child of ApplicationEntity
        /// </summary>
        [Test]
        public void ValidateThatCanGetOWnerApplicationDetailsForComponentWhenChildOfApplicationEntity()
        {
            var serviceResolver = Substitute.For<IServiceResolver>();
            var entityManager = new EntityManager(serviceResolver);

            Entity rootEntity = new Entity() { EntityManager = entityManager };
            entityManager.RootEntity = rootEntity;
            var applicationPoolEntity = new Entity();
            rootEntity.AddComponent(applicationPoolEntity);

            var applicationDetails = Substitute.For<IApplication>();

            var applicationEntity = Substitute.For<Entity, IApplicationEntity>();
            applicationEntity.Components.Returns(new List<IComponent>());           
            (applicationEntity as IApplicationEntity).GetTargetApplicationDetails().Returns(applicationDetails);
            (applicationEntity as IApplicationEntity).GetTargetApplicationDetails<IApplication>().Returns(applicationDetails);

            applicationPoolEntity.AddComponent(applicationEntity);

            var component = Substitute.For<IComponent>();
            component.Parent.Returns(applicationEntity);           

            var targetApplicationDetailsOne = entityManager.GetOwnerApplication<IApplication>(component);
            Assert.IsNotNull(targetApplicationDetailsOne);

            var targetApplicationDetailsTwo = entityManager.GetOwnerApplication(component);
            Assert.IsNotNull(targetApplicationDetailsTwo);

            Assert.AreSame(targetApplicationDetailsOne, targetApplicationDetailsTwo);
        }


        /// <summary>
        /// Validate that for a given component it's owner application details can be retrieved from process tree.
        /// The component is descendant of a sequence . Sequnce stores information of associated application id.
        /// The ApplicationEntity mapped to Sequence resides in a application pool.
        /// </summary>
        [Test]
        public void ValidateThatCanGetOwnerApplicationDetailsForComponentWhenDescendantOfSequence()
        {
            var serviceResolver = Substitute.For<IServiceResolver>();
            var entityManager = new EntityManager(serviceResolver);

            var rootEntity = new Entity() { EntityManager = entityManager };
            entityManager.RootEntity = rootEntity;
        
            var applicationPoolEntity = new Entity();
            rootEntity.AddComponent(applicationPoolEntity);

            var applicationDetails = Substitute.For<IApplication>();

            var applicationEntity = Substitute.For<Entity, IApplicationEntity>();
            (applicationEntity as IApplicationEntity).ApplicationId.Returns("MockId");
            applicationEntity.Components.Returns(new List<IComponent>());
            (applicationEntity as IApplicationEntity).GetTargetApplicationDetails().Returns(applicationDetails);
            (applicationEntity as IApplicationEntity).GetTargetApplicationDetails<IApplication>().Returns(applicationDetails);
            applicationPoolEntity.AddComponent(applicationEntity);


            var testCaseEntity = new Entity();
            rootEntity.AddComponent(testCaseEntity);
            var sequence = Substitute.For<Entity, IApplicationContext>();
            sequence.Components.Returns(new List<IComponent>());        
            (sequence as IApplicationContext).GetAppContext().Returns("MockId");
            testCaseEntity.AddComponent(sequence);
            var actorComponent = Substitute.For<ActorComponent>();
            actorComponent.Parent = sequence; 

            var ownerApplicationDetailsOne = entityManager.GetOwnerApplication<IApplication>(actorComponent);
            Assert.IsNotNull(ownerApplicationDetailsOne);


            var ownerApplicationDetailsTwo = entityManager.GetOwnerApplication<IApplication>(actorComponent);
            Assert.IsNotNull(ownerApplicationDetailsTwo);

            Assert.AreSame(ownerApplicationDetailsOne, ownerApplicationDetailsTwo);
        }

        /// <summary>
        /// Given a standard test case process setup where application entity in application pool has a control locator / coordinate provider available and a control identity 
        /// component needs control locator / coordinate provider , it should be able to retrieve control locator / coordinate provider from its mapped application.
        /// </summary>
        [Test]
        public void ValidateThatControlLocatorAndCoordinateProviderCanBeRetrieved()
        {

            var serviceResolver = Substitute.For<IServiceResolver>();
            var entityManager = new EntityManager(serviceResolver);

            var rootEntity = new Entity() { EntityManager = entityManager };
            entityManager.RootEntity = rootEntity;

            var applicationPoolEntity = new Entity();
            rootEntity.AddComponent(applicationPoolEntity);

            var controlLocator = Substitute.For<IControlLocator, IComponent>();
            controlLocator.CanProcessControlOfType(Arg.Any<IControlIdentity>()).Returns(true);
            var coordinateProvider = Substitute.For<ICoordinateProvider, IComponent>();
            coordinateProvider.CanProcessControlOfType(Arg.Any<IControlIdentity>()).Returns(true);       


            var applicationEntity = Substitute.For<Entity, IApplicationEntity>();
            (applicationEntity as IApplicationEntity).ApplicationId.Returns("MockId");
            applicationEntity.Components.Returns(new List<IComponent>() { controlLocator as IComponent, coordinateProvider as IComponent });
            applicationPoolEntity.AddComponent(applicationEntity);

          

            var testCaseEntity = new Entity();
            rootEntity.AddComponent(testCaseEntity);
            var sequence = new Entity();
            testCaseEntity.AddComponent(sequence);

            var controlIdentity = Substitute.For<IControlIdentity, IComponent>();
            controlIdentity.ApplicationId.Returns("MockId"); //while scraping a control for a application, application id is mapped and store
            sequence.AddComponent(controlIdentity as IComponent);

            var retrievedControlLocator = entityManager.GetControlLocator(controlIdentity);
            Assert.IsNotNull(retrievedControlLocator);

            //Typically same class will implement both IControlLocator and ICoordinateProvider
            var retrievedCoordinateProvider = entityManager.GetCoordinateProvider(controlIdentity);
            Assert.IsNotNull(retrievedCoordinateProvider);
        }
    }

}
