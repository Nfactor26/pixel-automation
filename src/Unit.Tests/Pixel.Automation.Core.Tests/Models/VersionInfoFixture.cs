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

            Assert.That(projectVersion.IsActive);
            Assert.That(projectVersion.IsPublished == false);
            Assert.That(string.IsNullOrEmpty(projectVersion.DataModelAssembly));
            Assert.That(string.IsNullOrEmpty(projectVersion.Description));
            Assert.That("1.0.0.0", Is.EqualTo(projectVersion.ToString()));
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

            Assert.That(projectVersion.IsActive == false);
            Assert.That(projectVersion.IsPublished);
            Assert.That("Assembly", Is.EqualTo(projectVersion.DataModelAssembly));
            Assert.That("Description", Is.EqualTo(projectVersion.Description));
            Assert.That("1.0.0.0", Is.EqualTo(projectVersion.ToString()));
        }
    }

    class PrefabVersionFixture
    {
        [Test]
        public void ValidatethatPrefabVersionCanBeInitializedFromVersionString()
        {
            var prefabVersion = new VersionInfo("1.0.0.0");

            Assert.That(prefabVersion.IsActive);
            Assert.That(prefabVersion.IsPublished == false);
            Assert.That(string.IsNullOrEmpty(prefabVersion.DataModelAssembly));
            Assert.That(string.IsNullOrEmpty(prefabVersion.Description));
            Assert.That("1.0.0.0", Is.EqualTo(prefabVersion.ToString()));
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

            Assert.That(prefabVersion.IsActive == false);
            Assert.That(prefabVersion.IsPublished);
            Assert.That("Assembly", Is.EqualTo(prefabVersion.DataModelAssembly));
            Assert.That("Description", Is.EqualTo(prefabVersion.Description));
            Assert.That("1.0.0.0", Is.EqualTo(prefabVersion.ToString()));
        }
    }
}
