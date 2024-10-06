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

            Assert.That("ApplicationId", Is.EqualTo(applicationDescription.ApplicationId));
            Assert.That("ApplicationName", Is.EqualTo(applicationDescription.ApplicationName));
            Assert.That(applicationDescription.ApplicationType is null);
            Assert.That(applicationDescription.Screens is not null);                             

            applicationDescription.ApplicationType = "WebApplication";
            Assert.That("WebApplication", Is.EqualTo(applicationDescription.ApplicationType));
        }


        //[Test]
        //public void ValidateThatControlCanBeAddedAndRemovedFromApplicationDescription()
        //{
        //    var applicationDescription = new ApplicationDescription();
        //    var controlDetails = Substitute.For<IControlIdentity>();
        //    controlDetails.ApplicationId.Returns("ApplicationId");
        //    var controlDescription = new ControlDescription(controlDetails);
        //    applicationDescription.AddControl(controlDescription);

        //    Assert.That(applicationDescription.AvailableControls.Contains(controlDescription.ControlId));
        //    Assert.That(applicationDescription.ControlsCollection.Contains(controlDescription));

        //    applicationDescription.DeleteControl(controlDescription);
        //    Assert.That(!applicationDescription.AvailableControls.Contains(controlDescription.ControlId));
        //    Assert.That(!applicationDescription.ControlsCollection.Contains(controlDescription));
        //}

        //[Test]
        //public void ValidateThatPrefabCanBeAddedAndRemovedFromApplicationDescription()
        //{
        //    var applicationDescription = new ApplicationDescription();
        //    var prefabProject = new PrefabProject() { ApplicationId = "A1", PrefabId = "P1" };
        //    applicationDescription.AddPrefab(prefabProject);

        //    Assert.That(applicationDescription.AvailablePrefabs.Contains(prefabProject.PrefabId));
        //    Assert.That(applicationDescription.PrefabsCollection.Contains(prefabProject));

        //    applicationDescription.DeletePrefab(prefabProject);
        //    Assert.That(!applicationDescription.AvailablePrefabs.Contains(prefabProject.PrefabId));
        //    Assert.That(!applicationDescription.PrefabsCollection.Contains(prefabProject));
        //}
    }
}
