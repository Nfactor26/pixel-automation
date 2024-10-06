using NUnit.Framework;
using Pixel.Automation.Core.Models;
using System.Linq;

namespace Pixel.Automation.Core.Tests.Models
{
    class AutomationProjectFixture
    {
        [Test]
        public void ValidateThatAutomationProjectCanBeInitialized()
        {
            var automationProject = new AutomationProject();
            Assert.That(automationProject.ProjectId is not null);
            Assert.That(string.IsNullOrEmpty(automationProject.Name));
            Assert.That(automationProject.AvailableVersions is not null);
            Assert.That(!automationProject.AvailableVersions.Any());
            Assert.That(automationProject.PublishedVersions is not null);
            Assert.That(!automationProject.PublishedVersions.Any());
            Assert.That(automationProject.LatestActiveVersion is null);          
        }

        [Test]
        public void ValidateThatNewVersionCanBeAddedToAutomationProject()
        {
            var automationProject = new AutomationProject();
            var deployedVersion = new VersionInfo(new System.Version(1, 0)) { IsActive = false };
            var activeVersion = new VersionInfo(new System.Version(1, 0)) { IsActive = true };
            automationProject.AvailableVersions.Add(deployedVersion);
            automationProject.AvailableVersions.Add(activeVersion);

            Assert.That(2, Is.EqualTo(automationProject.AvailableVersions.Count()));
            Assert.That(1, Is.EqualTo(automationProject.PublishedVersions.Count()));
            Assert.That(deployedVersion, Is.EqualTo(automationProject.PublishedVersions.First()));
            Assert.That(activeVersion, Is.EqualTo(automationProject.LatestActiveVersion));
        }
    }
}
