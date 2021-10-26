using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.AppExplorer.ViewModels.Tests
{
    /// <summary>
    /// Test Fixture for <see cref="ApplicationDescriptionViewModel"/>
    /// </summary>
    [TestFixture]
    public class ApplicationDescriptionViewModelFixture
    {      
        ApplicationDescription CreateApplicationDescription()
        {
            IApplication applicationDetails = Substitute.For<IApplication>();
            applicationDetails.ApplicationName.Returns("NotePad");
            applicationDetails.ApplicationId.Returns("application-id");           
            return new ApplicationDescription(applicationDetails)
            {
                ApplicationName = "NotePad",
                ApplicationDetails = applicationDetails
            };           
        }

        [Test]
        public void ValidateThatApplicatoinDescriptionViewModelCanBeCorrectlyInitialized()
        {
            var applicationDescription = CreateApplicationDescription();
            var applicationDescriptionViewModel = new ApplicationDescriptionViewModel(applicationDescription)
            {
                ApplicationType = "WinApplication"
            };

            Assert.IsNotNull(applicationDescriptionViewModel.Model);
            Assert.IsNotNull(applicationDescriptionViewModel.ApplicationDetails);
            Assert.AreEqual(applicationDescription.ApplicationId, applicationDescriptionViewModel.ApplicationId);
            Assert.AreEqual(applicationDescription.ApplicationName, applicationDescriptionViewModel.ApplicationName);
            Assert.AreEqual("WinApplication", applicationDescriptionViewModel.ApplicationType);
            Assert.AreEqual(0, applicationDescriptionViewModel.AvailableControls.Count);
            Assert.AreEqual(0, applicationDescriptionViewModel.AvailablePrefabs.Count);
            Assert.AreEqual(0, applicationDescriptionViewModel.ControlsCollection.Count);
            Assert.AreEqual(0, applicationDescriptionViewModel.PrefabsCollection.Count);
        }

        [Test]
        public void ValidateThatCanAddAndRemoveControl()
        {
            var applicationDescription = CreateApplicationDescription();
            var applicationDescriptionViewModel = new ApplicationDescriptionViewModel(applicationDescription)
            {
                ApplicationType = "WinApplication"
            };

            var control = new Control.ControlDescriptionViewModel(new ControlDescription());
            applicationDescriptionViewModel.AddControl(control);
                    
            Assert.AreEqual(1, applicationDescriptionViewModel.AvailableControls.Count);           
            Assert.AreEqual(1, applicationDescriptionViewModel.ControlsCollection.Count);

            applicationDescriptionViewModel.DeleteControl(control);

            Assert.AreEqual(0, applicationDescriptionViewModel.AvailableControls.Count);
            Assert.AreEqual(0, applicationDescriptionViewModel.ControlsCollection.Count);

        }

        [Test]
        public void ValidateThatCanAddAndRemovePrefab()
        {
            var applicationDescription = CreateApplicationDescription();
            var applicationDescriptionViewModel = new ApplicationDescriptionViewModel(applicationDescription)
            {
                ApplicationType = "WinApplication"
            };

            var prefab = new PrefabProject() { ApplicationId = "application-id", PrefabId = "prefab-id" };
            var prefabViewModel = new PrefabProjectViewModel(prefab);
            applicationDescriptionViewModel.AddPrefab(prefabViewModel);

            Assert.AreEqual(1, applicationDescriptionViewModel.AvailablePrefabs.Count);           
            Assert.AreEqual(1, applicationDescriptionViewModel.PrefabsCollection.Count);

            applicationDescriptionViewModel.DeletePrefab(prefabViewModel);

            Assert.AreEqual(0, applicationDescriptionViewModel.AvailablePrefabs.Count);
            Assert.AreEqual(0, applicationDescriptionViewModel.PrefabsCollection.Count);
        }
    }
}