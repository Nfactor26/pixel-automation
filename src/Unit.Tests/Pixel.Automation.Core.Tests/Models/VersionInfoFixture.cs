using NUnit.Framework;
using Pixel.Automation.Core.Models;
using System;

namespace Pixel.Automation.Core.Tests.Models
{
    class ProjectVersionFixture
    {
        [Test]
        public void ValidatethatProjectVersionCanBeInitializedFromVersionString()
        {
            var projectVersion = new ProjectVersion("1.0.0.0");

            Assert.IsTrue(projectVersion.IsActive);
            Assert.IsFalse(projectVersion.IsDeployed);
            Assert.IsTrue(string.IsNullOrEmpty(projectVersion.DataModelAssembly));
            Assert.IsTrue(string.IsNullOrEmpty(projectVersion.Description));
            Assert.AreEqual("1.0.0.0", projectVersion.ToString());
        }

        [Test]
        public void ValidatethatProjectVersionCanBeInitializedFromVersion()
        {
            var projectVersion = new ProjectVersion(new Version(1, 0, 0, 0))
            {
                IsActive = false,
                IsDeployed = true,
                DataModelAssembly = "Assembly",
                Description = "Description"
            };

            Assert.IsFalse(projectVersion.IsActive);
            Assert.IsTrue(projectVersion.IsDeployed);
            Assert.AreEqual("Assembly", projectVersion.DataModelAssembly);
            Assert.AreEqual("Description", projectVersion.Description);
            Assert.AreEqual("1.0.0.0", projectVersion.ToString());
        }
    }

    class PrefabVersionFixture
    {
        [Test]
        public void ValidatethatPrefabVersionCanBeInitializedFromVersionString()
        {
            var prefabVersion = new PrefabVersion("1.0.0.0");

            Assert.IsTrue(prefabVersion.IsActive);
            Assert.IsFalse(prefabVersion.IsDeployed);
            Assert.IsTrue(string.IsNullOrEmpty(prefabVersion.DataModelAssembly));
            Assert.IsTrue(string.IsNullOrEmpty(prefabVersion.Description));
            Assert.AreEqual("1.0.0.0", prefabVersion.ToString());
        }

        [Test]
        public void ValidatethatProjectVersionCanBeInitializedFromVersion()
        {
            var prefabVersion = new PrefabVersion(new Version(1, 0, 0, 0))
            {
                IsActive = false,
                IsDeployed = true,
                DataModelAssembly = "Assembly",
                Description = "Description"
            };

            Assert.IsFalse(prefabVersion.IsActive);
            Assert.IsTrue(prefabVersion.IsDeployed);
            Assert.AreEqual("Assembly", prefabVersion.DataModelAssembly);
            Assert.AreEqual("Description", prefabVersion.Description);
            Assert.AreEqual("1.0.0.0", prefabVersion.ToString());
        }
    }
}
