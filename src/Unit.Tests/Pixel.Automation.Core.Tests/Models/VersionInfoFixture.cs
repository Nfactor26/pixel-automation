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
            var projectVersion = new VersionInfo("1.0.0.0");

            Assert.IsTrue(projectVersion.IsActive);
            Assert.IsFalse(projectVersion.IsPublished);
            Assert.IsTrue(string.IsNullOrEmpty(projectVersion.DataModelAssembly));
            Assert.IsTrue(string.IsNullOrEmpty(projectVersion.Description));
            Assert.AreEqual("1.0.0.0", projectVersion.ToString());
        }

        [Test]
        public void ValidatethatProjectVersionCanBeInitializedFromVersion()
        {
            var projectVersion = new VersionInfo(new Version(1, 0, 0, 0))
            {
                IsActive = false,               
                DataModelAssembly = "Assembly",
                Description = "Description"
            };

            Assert.IsFalse(projectVersion.IsActive);
            Assert.IsTrue(projectVersion.IsPublished);
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
            var prefabVersion = new VersionInfo("1.0.0.0");

            Assert.IsTrue(prefabVersion.IsActive);
            Assert.IsFalse(prefabVersion.IsPublished);
            Assert.IsTrue(string.IsNullOrEmpty(prefabVersion.DataModelAssembly));
            Assert.IsTrue(string.IsNullOrEmpty(prefabVersion.Description));
            Assert.AreEqual("1.0.0.0", prefabVersion.ToString());
        }

        [Test]
        public void ValidatethatProjectVersionCanBeInitializedFromVersion()
        {
            var prefabVersion = new VersionInfo(new Version(1, 0, 0, 0))
            {
                IsActive = false,              
                DataModelAssembly = "Assembly",
                Description = "Description"
            };

            Assert.IsFalse(prefabVersion.IsActive);
            Assert.IsTrue(prefabVersion.IsPublished);
            Assert.AreEqual("Assembly", prefabVersion.DataModelAssembly);
            Assert.AreEqual("Description", prefabVersion.Description);
            Assert.AreEqual("1.0.0.0", prefabVersion.ToString());
        }
    }
}
