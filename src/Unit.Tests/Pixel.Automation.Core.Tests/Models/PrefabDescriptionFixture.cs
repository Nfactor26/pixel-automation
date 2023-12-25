using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Linq;

namespace Pixel.Automation.Core.Tests.Models
{
    class PrefabDescriptionFixture
    {
        [Test]
        public void ValidateThatPrefabDescriptionCanBeInitialized()
        {
            var prefabRoot = Substitute.For<IComponent>();
            var prefabProject = new PrefabProject(prefabRoot);

            Assert.AreEqual(null, prefabProject.ApplicationId);
            Assert.AreEqual(null, prefabProject.ProjectId);
            Assert.AreEqual(null, prefabProject.Name);
            Assert.AreEqual(null, prefabProject.Namespace);
            Assert.NotNull(prefabProject.AvailableVersions);
            Assert.NotNull(prefabProject.PublishedVersions);
            Assert.IsNull(prefabProject.LatestActiveVersion);
            Assert.AreEqual(null, prefabProject.Description);
            Assert.AreEqual("Default", prefabProject.GroupName);


            prefabProject.ApplicationId = "ApplicationId";
            prefabProject.ProjectId = "PrefabId";
            prefabProject.Name = "PrefabName";
            prefabProject.Namespace = $"{Constants.PrefabNameSpacePrefix}.PrefabName";
            prefabProject.AvailableVersions.Add(new VersionInfo() { IsActive = false, Version = new Version(1, 0) });
            prefabProject.AvailableVersions.Add(new VersionInfo() { IsActive = true, Version = new Version(2, 0) });
            prefabProject.Description = "Description";
            prefabProject.GroupName = "GroupName";


            Assert.AreEqual("ApplicationId", prefabProject.ApplicationId);
            Assert.AreEqual("PrefabId", prefabProject.ProjectId);
            Assert.AreEqual("PrefabName", prefabProject.Name);
            Assert.AreEqual($"{Constants.PrefabNameSpacePrefix}.PrefabName", prefabProject.Namespace);
            Assert.AreEqual(2, prefabProject.AvailableVersions.Count());
            Assert.AreEqual(1, prefabProject.PublishedVersions.Count());
            Assert.NotNull(prefabProject.LatestActiveVersion);
            Assert.AreEqual("Description", prefabProject.Description);
            Assert.AreEqual("GroupName", prefabProject.GroupName);
        }

        [Test]
        public void ValdiateThatPrefabDescriptionCanBeCorrectlyCloned()
        {
            var prefabRoot = Substitute.For<IComponent, ICloneable>();
            (prefabRoot as ICloneable).Clone().Returns(prefabRoot);

            var prefabDescription = new PrefabProject(prefabRoot)
            {
                ApplicationId = "ApplicationId",
                ProjectId = "PrefabId",
                Name = "PrefabName",
                Description = "Description",
                GroupName = "GroupName",
            };

            var clone = prefabDescription .Clone() as PrefabProject;

            Assert.AreEqual("ApplicationId", clone.ApplicationId);
            Assert.AreNotEqual("PrefabId", clone.ProjectId);
            Assert.AreEqual("PrefabName", clone.Name);
            Assert.AreEqual("Description", clone.Description);
            Assert.AreEqual("GroupName", clone.GroupName);
            Assert.AreSame(prefabRoot, clone.PrefabRoot);
            Assert.AreNotSame(prefabDescription.AvailableVersions, clone.AvailableVersions);
        }
    }
}
