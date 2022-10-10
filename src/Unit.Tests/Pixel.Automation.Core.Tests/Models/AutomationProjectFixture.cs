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
            Assert.IsNotNull(automationProject.LastOpened);
            Assert.IsNotNull(automationProject.AvailableVersions);
            Assert.IsTrue(!automationProject.AvailableVersions.Any());
            Assert.IsNotNull(automationProject.DeployedVersions);
            Assert.IsTrue(!automationProject.DeployedVersions.Any());
            Assert.IsNull(automationProject.ActiveVersion);          
        }

        [Test]
        public void ValidateThatNewVersionCanBeAddedToAutomationProject()
        {
            var automationProject = new AutomationProject();
            var deployedVersion = new ProjectVersion(new System.Version(1, 0)) { IsDeployed = true };
            var activeVersion = new ProjectVersion(new System.Version(1, 0)) { IsActive = true };
            automationProject.AvailableVersions.Add(deployedVersion);
            automationProject.AvailableVersions.Add(activeVersion);

            Assert.AreEqual(2, automationProject.AvailableVersions.Count());
            Assert.AreEqual(1, automationProject.DeployedVersions.Count());
            Assert.AreEqual(deployedVersion, automationProject.DeployedVersions.First());
            Assert.AreEqual(activeVersion, automationProject.ActiveVersion);
        }
    }
}
