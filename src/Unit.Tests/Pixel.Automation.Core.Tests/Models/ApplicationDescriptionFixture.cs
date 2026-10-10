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
    }
}
