using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using System;

namespace Pixel.Automation.Core.Tests.Models
{
    class ControlDescriptionFixture
    {
        [Test]
        public void ValidateThatControlDescriptionCanBeInitialized()
        {
            var control = Substitute.For<IControlIdentity, IComponent>();
            control.ApplicationId.Returns("ApplicationId");                   
            var controlDescription = new ControlDescription(control);

            Assert.That("ApplicationId", Is.EqualTo(controlDescription.ApplicationId));
            Assert.That(controlDescription.ControlId is not null);
            Assert.That(!string.IsNullOrEmpty(controlDescription.ControlId));
            Assert.That(control, Is.EqualTo(controlDescription.ControlDetails));
            Assert.That(controlDescription.ControlName is null);
            Assert.That("Default", Is.EqualTo(controlDescription.GroupName));
            Assert.That(controlDescription.ControlImage is null);

            controlDescription.ControlName = "ControlName";
            controlDescription.ControlImage = "ControlImage";
            controlDescription.GroupName = "GroupName";

            Assert.That("ControlName", Is.EqualTo(controlDescription.ControlName));
            Assert.That("ControlImage", Is.EqualTo(controlDescription.ControlImage));
            Assert.That("GroupName", Is.EqualTo(controlDescription.GroupName));
        }

        [Test]
        public void ValidateThatControlDescriptionCanBeCorrectlyCloned()
        {
            var controlDetails = Substitute.For<IControlIdentity, ICloneable>();
            controlDetails.ApplicationId.Returns("ApplicationId");
            controlDetails.Clone().Returns(controlDetails);
            var controlDescription = new ControlDescription(controlDetails)
            {
                ControlName = "ControlName",
                GroupName = "GroupName",
                ControlImage = "ControlImage"
            };

            var clone = controlDescription.Clone() as ControlDescription;
            Assert.That(controlDescription.ControlName, Is.EqualTo(clone.ControlName));
            Assert.That(controlDescription.ControlImage, Is.EqualTo(clone.ControlImage));
            Assert.That(controlDescription.GroupName, Is.EqualTo(clone.GroupName));
            Assert.That(controlDescription.ControlDetails, Is.EqualTo(clone.ControlDetails));
            Assert.That(clone.ControlId is not null);
            Assert.That(!string.IsNullOrEmpty(clone.ControlId));
        }
    }
}
