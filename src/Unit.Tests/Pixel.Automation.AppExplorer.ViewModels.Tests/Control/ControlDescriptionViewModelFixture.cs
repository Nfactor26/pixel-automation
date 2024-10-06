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

            Assert.That(controlDescriptionViewModel.ControlDescription, Is.SameAs(controlDescription));
            Assert.That(controlDescriptionViewModel.ControlDetails, Is.SameAs(controlDescription.ControlDetails));
            Assert.That(controlDescriptionViewModel.ApplicationId, Is.EqualTo(controlDescription.ApplicationId));
            Assert.That(controlDescriptionViewModel.ControlId, Is.EqualTo(controlDescription.ControlId));
            Assert.That(controlDescriptionViewModel.GroupName, Is.EqualTo(controlDescription.GroupName));
            Assert.That(controlDescriptionViewModel.ControlName, Is.EqualTo(controlDescription.ControlName));
            Assert.That(controlDescriptionViewModel.ControlImage, Is.EqualTo(controlDescription.ControlImage));
            Assert.That(controlDescriptionViewModel.ToString(), Is.EqualTo("Control Details"));

            controlDescriptionViewModel.ControlName = "AddButton";
            controlDescriptionViewModel.GroupName = "Buttons";
            controlDescriptionViewModel.ControlImage = "AddButton.Png";

            Assert.That(controlDescriptionViewModel.GroupName, Is.EqualTo("Buttons"));
            Assert.That(controlDescriptionViewModel.ControlName, Is.EqualTo("AddButton"));
            Assert.That(controlDescriptionViewModel.ControlImage, Is.EqualTo("AddButton.Png"));
        }
    }
}
