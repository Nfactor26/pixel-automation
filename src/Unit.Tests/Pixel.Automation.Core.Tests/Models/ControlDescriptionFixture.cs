using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
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

            Assert.AreEqual("ApplicationId", controlDescription.ApplicationId);
            Assert.IsNotNull(controlDescription.ControlId);
            Assert.IsNotEmpty(controlDescription.ControlId);
            Assert.AreEqual(control, controlDescription.ControlDetails);
            Assert.IsNull(controlDescription.ControlName);
            Assert.AreEqual("Default", controlDescription.GroupName);
            Assert.IsNull(controlDescription.ControlImage);

            controlDescription.ControlName = "ControlName";
            controlDescription.ControlImage = "ControlImage";
            controlDescription.GroupName = "GroupName";

            Assert.AreEqual("ControlName", controlDescription.ControlName);
            Assert.AreEqual("ControlImage", controlDescription.ControlImage);
            Assert.AreEqual("GroupName", controlDescription.GroupName);
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
            Assert.AreEqual(controlDescription.ControlName, clone.ControlName);
            Assert.AreEqual(controlDescription.ControlImage, clone.ControlImage);
            Assert.AreEqual(controlDescription.GroupName, clone.GroupName);
            Assert.AreEqual(controlDescription.ControlDetails, clone.ControlDetails);
            Assert.IsNotNull(clone.ControlId);
            Assert.IsNotEmpty(clone.ControlId);
        }
    }
}
