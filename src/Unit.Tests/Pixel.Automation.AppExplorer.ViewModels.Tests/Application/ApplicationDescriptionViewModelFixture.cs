using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System.Linq;

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
        public void ValidateThatApplicationDescriptionViewModelCanBeCorrectlyInitialized()
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
            Assert.AreEqual(0, applicationDescriptionViewModel.Screens.Count);           
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
            applicationDescriptionViewModel.AddScreen(new ApplicationScreen("Home"));

            var control = new Control.ControlDescriptionViewModel(new ControlDescription());
            applicationDescriptionViewModel.AddControl(control, "Home");
                    
            Assert.AreEqual(1, applicationDescriptionViewModel["Home"].AvailableControls.Count);           
            Assert.AreEqual(1, applicationDescriptionViewModel.ControlsCollection.Count);
            Assert.AreEqual(1, applicationDescriptionViewModel.ScreenCollection.Screens.Count());

            applicationDescriptionViewModel.DeleteControl(control, "Home");

            Assert.AreEqual(0, applicationDescriptionViewModel["Home"].AvailableControls.Count);
            Assert.AreEqual(0, applicationDescriptionViewModel.ControlsCollection.Count);

            applicationDescriptionViewModel.DeleteScreen("Home");
            Assert.AreEqual(0, applicationDescriptionViewModel.ScreenCollection.Screens.Count());

        }

        [Test]
        public void ValidateThatCanAddAndRemovePrefab()
        {
            var applicationDescription = CreateApplicationDescription();
            var applicationDescriptionViewModel = new ApplicationDescriptionViewModel(applicationDescription)
            {
                ApplicationType = "WinApplication"
            };
            applicationDescriptionViewModel.AddScreen(new ApplicationScreen("Home"));

            var prefab = new PrefabProject() { ApplicationId = "application-id", ProjectId = "prefab-id" };
            var prefabViewModel = new PrefabProjectViewModel(prefab);
            applicationDescriptionViewModel.AddPrefab(prefabViewModel, "Home");
         
            Assert.AreEqual(1, applicationDescriptionViewModel["Home"].AvailablePrefabs.Count);
            Assert.AreEqual(1, applicationDescriptionViewModel.PrefabsCollection.Count);

            applicationDescriptionViewModel.DeletePrefab(prefabViewModel, "Home");
      
            Assert.AreEqual(0, applicationDescriptionViewModel["Home"].AvailablePrefabs.Count);
            Assert.AreEqual(0, applicationDescriptionViewModel.PrefabsCollection.Count);
        }
    }
}