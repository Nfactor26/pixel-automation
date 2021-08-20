using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Core.Tests.Models
{
    class ApplicationDescriptionFixture
    {
        [Test]
        public void ValidateThatApplicationDescriptionCanBeInitialized()
        {
            var application = Substitute.For<IApplication>();
            application.ApplicationId.Returns("ApplicationId");
            application.ApplicationName.Returns("ApplicationName");
            var applicationDescription = new ApplicationDescription(application);

            Assert.AreEqual("ApplicationId", applicationDescription.ApplicationId);
            Assert.AreEqual("ApplicationName", applicationDescription.ApplicationName);
            Assert.IsNull(applicationDescription.ApplicationType);
            Assert.IsNotNull(applicationDescription.AvailableControls);
            Assert.IsNotNull(applicationDescription.ControlsCollection);
            Assert.IsNotNull(applicationDescription.AvailablePrefabs);
            Assert.IsNotNull(applicationDescription.PrefabsCollection);

            applicationDescription.ApplicationType = "WebApplication";
            Assert.AreEqual("WebApplication", applicationDescription.ApplicationType);
        }


        [Test]
        public void ValidateThatControlCanBeAddedAndRemovedFromApplicationDescription()
        {
            var applicationDescription = new ApplicationDescription();
            var controlDetails = Substitute.For<IControlIdentity>();
            controlDetails.ApplicationId.Returns("ApplicationId");
            var controlDescription = new ControlDescription(controlDetails);
            applicationDescription.AddControl(controlDescription);

            Assert.IsTrue(applicationDescription.AvailableControls.Contains(controlDescription.ControlId));
            Assert.IsTrue(applicationDescription.ControlsCollection.Contains(controlDescription));

            applicationDescription.DeleteControl(controlDescription);
            Assert.IsTrue(!applicationDescription.AvailableControls.Contains(controlDescription.ControlId));
            Assert.IsTrue(!applicationDescription.ControlsCollection.Contains(controlDescription));
        }

        [Test]
        public void ValidateThatPrefabCanBeAddedAndRemovedFromApplicationDescription()
        {
            var applicationDescription = new ApplicationDescription();
            var prefabProject = new PrefabProject() { ApplicationId = "A1", PrefabId = "P1" };
            applicationDescription.AddPrefab(prefabProject);

            Assert.IsTrue(applicationDescription.AvailablePrefabs.Contains(prefabProject.PrefabId));
            Assert.IsTrue(applicationDescription.PrefabsCollection.Contains(prefabProject));

            applicationDescription.DeletePrefab(prefabProject);
            Assert.IsTrue(!applicationDescription.AvailablePrefabs.Contains(prefabProject.PrefabId));
            Assert.IsTrue(!applicationDescription.PrefabsCollection.Contains(prefabProject));
        }
    }
}
