using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.AppExplorer.ViewModels.Control;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.AppExplorer.ViewModels.Tests
{
    /// <summary>
    /// Test Fixture for <see cref="ControlDescriptionViewModel"/>
    /// </summary>
    [TestFixture]
    public class ControlDescriptionViewModelFixture
    {
        [Test]
        public void ValidateThatControlDescriptionViewModelCanBeCorrectlyInitialized()
        {
            var controlIdentity = Substitute.For<IControlIdentity>();
            controlIdentity.ApplicationId.Returns("application-id");         
            var controlDescription = new ControlDescription(controlIdentity)
            {
                ControlName = "SaveButton",
                GroupName = "Default"
            };

            var controlDescriptionViewModel = new ControlDescriptionViewModel(controlDescription);

            Assert.AreSame(controlDescription, controlDescriptionViewModel.ControlDescription);
            Assert.AreSame(controlDescription.ControlDetails, controlDescriptionViewModel.ControlDetails);
            Assert.AreEqual(controlDescription.ApplicationId, controlDescriptionViewModel.ApplicationId);
            Assert.AreEqual(controlDescription.ControlId, controlDescriptionViewModel.ControlId);
            Assert.AreEqual(controlDescription.GroupName, controlDescriptionViewModel.GroupName);
            Assert.AreEqual(controlDescription.ControlName, controlDescriptionViewModel.ControlName);
            Assert.AreEqual(controlDescription.ControlImage, controlDescriptionViewModel.ControlImage);
            Assert.AreEqual("Control Details", controlDescriptionViewModel.ToString());

            controlDescriptionViewModel.ControlName = "AddButton";
            controlDescriptionViewModel.GroupName = "Buttons";
            controlDescriptionViewModel.ControlImage = "AddButton.Png";

            Assert.AreEqual("Buttons", controlDescriptionViewModel.GroupName);
            Assert.AreEqual("AddButton", controlDescriptionViewModel.ControlName);
            Assert.AreEqual("AddButton.Png", controlDescriptionViewModel.ControlImage);
        }
    }
}
