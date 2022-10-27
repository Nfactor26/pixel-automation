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
            Assert.IsNotNull(automationProject.ProjectId);
            Assert.IsEmpty(automationProject.Name);
            Assert.IsNotNull(automationProject.AvailableVersions);
            Assert.IsTrue(!automationProject.AvailableVersions.Any());
            Assert.IsNotNull(automationProject.PublishedVersions);
            Assert.IsTrue(!automationProject.PublishedVersions.Any());
            Assert.IsNull(automationProject.LatestActiveVersion);          
        }

        [Test]
        public void ValidateThatNewVersionCanBeAddedToAutomationProject()
        {
            var automationProject = new AutomationProject();
            var deployedVersion = new ProjectVersion(new System.Version(1, 0)) { IsActive = false };
            var activeVersion = new ProjectVersion(new System.Version(1, 0)) { IsActive = true };
            automationProject.AvailableVersions.Add(deployedVersion);
            automationProject.AvailableVersions.Add(activeVersion);

            Assert.AreEqual(2, automationProject.AvailableVersions.Count());
            Assert.AreEqual(1, automationProject.PublishedVersions.Count());
            Assert.AreEqual(deployedVersion, automationProject.PublishedVersions.First());
            Assert.AreEqual(activeVersion, automationProject.LatestActiveVersion);
        }
    }
}
