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
using System.Reflection;

namespace Pixel.Automation.Core.Tests
{
    public class ArgumentDataModel
    {
        public int Count { get; set; }
    }

    public class EntityManagerFixture
    {
        private IServiceResolver serviceResolver;
        private IFileSystem fileSystem;       
        private IScriptEngineFactory scriptEngineFactory;     
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
            this.serializer = Substitute.For<ISerializer>();

            this.fileSystem.ReferenceManager.Returns(new AssemblyReferenceManager(new ApplicationSettings(), string.Empty, string.Empty));

            this.scriptEngineFactory = Substitute.For<IScriptEngineFactory>();
            this.scriptEngineFactory.CreateScriptEngine(Arg.Any<string>()).Returns((a) => { return CreateScriptEngine(); });

            this.syntheticMouse = Substitute.For<ISyntheticMouse>();
            this.syntheticKeyboard = Substitute.For<ISyntheticKeyboard>();

            serviceResolver.Get<IServiceResolver>().Returns(this.serviceResolver);
            serviceResolver.Get<IArgumentProcessor>().Returns((a) => { return CreateArgumentProcessor();});
            serviceResolver.Get<IFileSystem>().Returns(this.fileSystem);
            serviceResolver.Get<ISerializer>().Returns(this.serializer);
            serviceResolver.Get<IScriptEngineFactory>().Returns(this.scriptEngineFactory);     
            serviceResolver.Get<ISyntheticKeyboard>().Returns(this.syntheticKeyboard as ISyntheticKeyboard);
            serviceResolver.Get<ISyntheticMouse>().Returns(this.syntheticMouse as ISyntheticMouse);
            serviceResolver.GetAll<IDevice>().Returns(new List<IDevice>() { this.syntheticKeyboard, this.syntheticMouse });
            serviceResolver.When(x => x.RegisterDefault<EntityManager>(Arg.Any<EntityManager>())).Do((a) => { });
            serviceResolver.When(d => d.Dispose()).Do(x => { });

            this.dataModel = new Person();

            this.entityManager = new EntityManager(serviceResolver);
            this.entityManager.SetCurrentFileSystem(fileSystem);
            this.entityManager.Arguments = this.dataModel;

            this.scriptEngineFactory.Received(1).WithSearchPaths(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            this.scriptEngineFactory.Received(1).CreateScriptEngine(Arg.Any<string>());
            this.entityManager.GetScriptEngine().Received(1).SetGlobals(Arg.Any<object>());
            this.entityManager.GetArgumentProcessor().Received(1).Initialize(Arg.Any<IScriptEngine>(), Arg.Any<object>());

            IArgumentProcessor CreateArgumentProcessor()
            {
                return Substitute.For<IArgumentProcessor>();
            }

            IScriptEngine CreateScriptEngine()
            {
                var scriptEngine = Substitute.For<IScriptEngine>();
                var propertyCollection = new List<PropertyDescription>();
                propertyCollection.Add(new PropertyDescription("PropertyOne", typeof(string)));
                propertyCollection.Add(new PropertyDescription("PropertyTwo", typeof(int)));
                scriptEngine.GetScriptVariables().Returns(
                    propertyCollection
                 );
                return scriptEngine;
            }

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
            Assert.AreSame(this.syntheticMouse, entityManager.GetServiceOfType<ISyntheticMouse>());
            Assert.AreSame(this.syntheticKeyboard, entityManager.GetServiceOfType<ISyntheticKeyboard>());

            this.serviceResolver.Received(1).Get<ISerializer>();
            this.serviceResolver.Received(1).Get<IFileSystem>();           
            this.serviceResolver.Received(1).Get<ISyntheticMouse>();
            this.serviceResolver.Received(1).Get<ISyntheticKeyboard>();
        }


        [Test]
        [Order(25)]
        public void VerifyThatFileSystemCanBeRetrieved()
        {
            var fileSystem = entityManager.GetCurrentFileSystem();
            Assert.AreSame(this.fileSystem, fileSystem);
        }


        [Test]
        [Order(30)]
        public void VerifyThatArgumentProcessorCanBeRetrieved()
        {
            var argumentProcessor = entityManager.GetArgumentProcessor();
            Assert.IsNotNull(argumentProcessor);
        }


        [Test]
        [Order(40)]
        public void VerifyThatScriptEngineCanBeRetrieved()
        {
            var scriptEngine = entityManager.GetScriptEngine();
            Assert.IsNotNull(scriptEngine);
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
        public void ValidateThatDefaultsForServicesCanBeRegisteredWithEntityManager()
        {
            this.entityManager.RegisterDefault<ISerializer>(this.serializer);
            this.serviceResolver.Received(1).RegisterDefault<ISerializer>(Arg.Is<ISerializer>(this.serializer));
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

        /// <summary>
        /// Verify that Secondary EntityManager can be created from existing ExistingEntityManager and they should share some of the services while have their own implementations
        /// of few services. Shared vs non-shared services will depend on how are they registered with the underlying service resolver.
        /// </summary>
        [Test]
        [Order(110)]
        public void ValidateThatEntityManagerCanBeCreatedFromExistingEntityManager()
        {
            Entity rootEntity = new Entity("Root", "Root"); 
            this.entityManager.RootEntity = rootEntity;
            var secondaryEntityManager = new EntityManager(this.entityManager);          
            Assert.AreSame(secondaryEntityManager.RootEntity, this.entityManager.RootEntity);
            Assert.AreSame(secondaryEntityManager.GetCurrentFileSystem(), this.entityManager.GetCurrentFileSystem());
            Assert.AreSame(this.entityManager, rootEntity.EntityManager);
         
            secondaryEntityManager.Arguments = new Person();

            Assert.AreSame(secondaryEntityManager.GetServiceOfType<IScriptEngineFactory>(), this.entityManager.GetServiceOfType<IScriptEngineFactory>());
            Assert.AreNotSame(secondaryEntityManager.GetScriptEngine(), this.entityManager.GetScriptEngine());
            Assert.AreNotSame(secondaryEntityManager.GetArgumentProcessor(), this.entityManager.GetArgumentProcessor());
        }

        /// <summary>
        /// Validate that Arguments can be updated for an EntityManager. Also, updating the arguments should re-cofigure or re-initialize some of the underlying services which 
        /// make use of the arguments
        /// </summary>
        [Test]
        [Order(120)]
        public void ValidateThatArgumentsCanBeUpdatedForEntityManagerAndArgumentDependentServicesAreAutomaticallyReConfigured()
        {
            this.entityManager.GetScriptEngine().ClearReceivedCalls();
            this.entityManager.GetArgumentProcessor().ClearReceivedCalls();

            //we are purposely setting new argument to a type defined in different assembly from initial type of Person
            var newArgument = new ArgumentDataModel() { Count = 5 };
            this.entityManager.Arguments = newArgument;
            this.scriptEngineFactory.Received(1).RemoveReferences(Arg.Is<Assembly>(typeof(Person).Assembly));
            this.scriptEngineFactory.Received(1).WithAdditionalAssemblyReferences(Arg.Is<Assembly>(typeof(ArgumentDataModel).Assembly));
           
            var scriptEngine = this.entityManager.GetScriptEngine();          
            scriptEngine.Received(1).SetGlobals(Arg.Any<object>());

            var argumentProcessor = this.entityManager.GetArgumentProcessor();        
            argumentProcessor.Received(1).Initialize(Arg.Any<IScriptEngine>(), Arg.Any<object>());
        }


        [Test]
        [Order(130)]
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
            Assert.IsNull(entityManager.RootEntity);
            Assert.IsNull(entityManager.Arguments);
        }



        /// <summary>
        /// Validate that for a given component it's owner application details can be retrieved using GetOwnerApplication() from EntityManager
        /// when component is a child of ApplicationEntity
        /// </summary>
        [Test]
        [Order(200)]
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
        [Order(210)]
        public void ValidateThatCanGetOwnerApplicationDetailsForComponentWhenDescendantOfSequence()
        {
            var serviceResolver = Substitute.For<IServiceResolver>();
            var entityManager = new EntityManager(serviceResolver);

            var rootEntity = new Entity() { EntityManager = entityManager };
            entityManager.RootEntity = rootEntity;

            var applicationPoolEntity = new Entity("Application Pool", "ApplicationPoolEntity");
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
        [Order(220)]
        public void ValidateThatControlLocatorAndCoordinateProviderCanBeRetrieved()
        {

            var serviceResolver = Substitute.For<IServiceResolver>();
            var entityManager = new EntityManager(serviceResolver);

            var rootEntity = new Entity() { EntityManager = entityManager };
            entityManager.RootEntity = rootEntity;

            var applicationPoolEntity = new Entity("Application Pool", "ApplicationPoolEntity");
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
