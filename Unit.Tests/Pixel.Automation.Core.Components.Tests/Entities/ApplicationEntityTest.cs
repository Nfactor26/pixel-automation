using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Components.Sequences;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Core.Components.Tests
{
    public class ApplicationEntityTest
    {

        private ApplicationEntity applicationEntity;
        private ISerializer serializer;
        private IFileSystem fileSystem;
        private IEntityManager entityManager;

        [OneTimeSetUp]
        public void Setup()
        {
            entityManager = Substitute.For<IEntityManager>();

            fileSystem = Substitute.For<IFileSystem>();
            fileSystem.Exists(Arg.Any<string>()).Returns(true);
            entityManager.GetCurrentFileSystem().Returns(fileSystem);

            serializer = Substitute.For<ISerializer>();
            var applicationDetails = Substitute.For<IApplication, IComponent>();
            var applicationDescription = new ApplicationDescription()
            {               
                ApplicationName = "MockApplication",
                ApplicationType = "Windows",
                ApplicationDetails = applicationDetails
            };
            serializer.Deserialize<ApplicationDescription>(Arg.Any<string>(), null).Returns(applicationDescription);
            entityManager.GetServiceOfType<ISerializer>().Returns(serializer);

            applicationEntity = new ApplicationEntity() { ApplicationId = "MockId", EntityManager = entityManager };          

            var controlLocator = Substitute.For<IControlLocator, IComponent>();
            controlLocator.CanProcessControlOfType(Arg.Any<IControlIdentity>()).Returns(true);
            var coordinateProvider = Substitute.For<ICoordinateProvider, IComponent>();
            coordinateProvider.CanProcessControlOfType(Arg.Any<IControlIdentity>()).Returns(true);
            applicationEntity.AddComponent(controlLocator as IComponent);
            applicationEntity.AddComponent(coordinateProvider as IComponent);
        }

        [Test]
        public void ValidateThatApplicationDetailsCanBeRetrievedFromApplicationEntity()
        {
            var targetApplicationDetails = applicationEntity.GetTargetApplicationDetails<IApplication>();
            Assert.NotNull(targetApplicationDetails);

            //second time application details is returned as it is since it was already loaded earlier
            targetApplicationDetails = applicationEntity.GetTargetApplicationDetails();
            Assert.NotNull(targetApplicationDetails);

            //serializer should receive exactly one call since application details is loaded once and then reused
            serializer.Received(1).Deserialize<ApplicationDescription>(Arg.Any<string>(), null);
            fileSystem.Received(1).Exists(Arg.Any<string>());
        }

        /// <summary>
        /// Validate that for a given component it's owner application details can be retrieved using extension method GetApplicationDetails() available on EntityManager
        /// when component is a child of ApplicationEntity
        /// </summary>
        [Test]
        public void ValidateThatCanGetOWnerApplicationDetailsForComponentWhenChildOfApplicationEntity()
        {
            var component = Substitute.For<IComponent>();
            applicationEntity.AddComponent(component);

            var targetApplicationDetailsOne = entityManager.GetApplicationDetails<IApplication>(component);
            Assert.IsNotNull(targetApplicationDetailsOne);

            var targetApplicationDetailsTwo = entityManager.GetApplicationDetails(component);
            Assert.IsNotNull(targetApplicationDetailsTwo);

            Assert.AreSame(targetApplicationDetailsOne, targetApplicationDetailsTwo);
        }

        /// <summary>
        /// Validate that for a given component it's owner application details can be retrieved from process tree.
        /// The component is descendant of a sequence . Sequnce stores information of associated application id.
        /// The ApplicationEntity mapped to Sequence resides in a application pool
        /// </summary>
        [Test]
        public void ValidateThatCanGetOwnerApplicationDetailsForComponentWhenDescendantOfSequence()
        {
            var rootEntity = new Entity() { EntityManager = entityManager };
            entityManager.RootEntity = rootEntity;
            var appPoolEntity = new ApplicationPoolEntity();
            rootEntity.AddComponent(appPoolEntity);
            appPoolEntity.AddComponent(applicationEntity);
          
            var testCaseEntity = new Entity();
            rootEntity.AddComponent(testCaseEntity);
            var sequence = new SequenceEntity() { TargetAppId = "MockId" };
            testCaseEntity.AddComponent(sequence);
            var actorComponent = Substitute.For<ActorComponent>();
            sequence.AddComponent(actorComponent);

            var ownerApplicationDetailsOne = entityManager.GetApplicationDetails<IApplication>(actorComponent);
            Assert.IsNotNull(ownerApplicationDetailsOne);


            var ownerApplicationDetailsTwo = entityManager.GetApplicationDetails<IApplication>(actorComponent);
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
            var rootEntity = new Entity() { EntityManager = entityManager };
            entityManager.RootEntity = rootEntity;
            var appPoolEntity = new ApplicationPoolEntity();
            rootEntity.AddComponent(appPoolEntity);
            appPoolEntity.AddComponent(applicationEntity);

            var testCaseEntity = new Entity();
            rootEntity.AddComponent(testCaseEntity);
            var sequence = new SequenceEntity();
            testCaseEntity.AddComponent(sequence);

            var controlIdentity = Substitute.For<IControlIdentity, IComponent>();
            controlIdentity.ApplicationId.Returns("MockId"); //while scraping a control for a application, application id is mapped and store
            sequence.AddComponent(controlIdentity as IComponent);

            var controlLocator = entityManager.GetControlLocator(controlIdentity);
            Assert.IsNotNull(controlLocator);

            //Typically same class will implement both IControlLocator and ICoordinateProvider
            var coordinateProvider = entityManager.GetCoordinateProvider(controlIdentity);
            Assert.IsNotNull(coordinateProvider);
        } 
        

        /// <summary>
        /// Verify that EntityManager can retrieve an ApplicationEntity using its id given a standard test process setup
        /// </summary>
        /// <returns></returns>
        [TestCase("MockId")]
        [TestCase("")]
        public void ValidateThatApplicationEntityCanBeRetrievedGivenItsId(string applicationId)
        {
            var rootEntity = new Entity() { EntityManager = entityManager };
            entityManager.RootEntity = rootEntity;
            var appPoolEntity = new ApplicationPoolEntity();
            rootEntity.AddComponent(appPoolEntity);
            appPoolEntity.AddComponent(applicationEntity);

            if(!string.IsNullOrEmpty(applicationId))
            {
                var appEntity = entityManager.GetApplicationEntityByApplicationId(applicationId);
                Assert.IsNotNull(appEntity);
                Assert.AreEqual(applicationEntity, appEntity);
            }
            else
            {
                Assert.Throws<ConfigurationException>(() => { entityManager.GetApplicationEntityByApplicationId(applicationId); });
            }
        }
    }
}
