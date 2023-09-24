using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Tests
{
    public class ApplicationEntityTest
    {
        class MockApplicationEntity : ApplicationEntity
        {
            public override async Task LaunchAsync()
            {
                throw new System.NotImplementedException();
            }

            public override async Task CloseAsync()
            {
                throw new System.NotImplementedException();
            }          

            public override async Task CaptureScreenShotAsync(string filePath)
            {
                throw new System.NotImplementedException();
            }
        }

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
            applicationDetails.ApplicationName.Returns("MockApplication");           
            var applicationDescription = new ApplicationDescription(applicationDetails)
            {               
                ApplicationType = "Windows"              
            };
            serializer.Deserialize<ApplicationDescription>(Arg.Any<string>(), null).Returns(applicationDescription);
            entityManager.GetServiceOfType<ISerializer>().Returns(serializer);

            applicationEntity = new MockApplicationEntity() { ApplicationId = "MockId", EntityManager = entityManager };      
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
      
    }
}
