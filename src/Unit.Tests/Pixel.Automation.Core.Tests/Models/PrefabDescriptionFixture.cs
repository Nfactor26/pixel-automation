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

            Assert.That(prefabProject.ApplicationId, Is.EqualTo(null));
            Assert.That(prefabProject.ProjectId is not null);
            Assert.That(prefabProject.Name, Is.EqualTo(null));
            Assert.That(prefabProject.Namespace, Is.EqualTo(null));
            Assert.That(prefabProject.AvailableVersions is not null);
            Assert.That(prefabProject.PublishedVersions is not null);
            Assert.That(prefabProject.LatestActiveVersion is null);
            Assert.That(prefabProject.Description, Is.EqualTo(null));
            Assert.That(prefabProject.GroupName, Is.EqualTo("Default"));


            prefabProject.ApplicationId = "ApplicationId";
            prefabProject.ProjectId = "PrefabId";
            prefabProject.Name = "PrefabName";
            prefabProject.Namespace = $"{Constants.PrefabNameSpacePrefix}.PrefabName";
            prefabProject.AvailableVersions.Add(new VersionInfo() { IsActive = false, Version = new Version(1, 0) });
            prefabProject.AvailableVersions.Add(new VersionInfo() { IsActive = true, Version = new Version(2, 0) });
            prefabProject.Description = "Description";
            prefabProject.GroupName = "GroupName";


            Assert.That(prefabProject.ApplicationId, Is.EqualTo("ApplicationId"));
            Assert.That(prefabProject.ProjectId, Is.EqualTo("PrefabId"));
            Assert.That(prefabProject.Name, Is.EqualTo("PrefabName"));
            Assert.That(prefabProject.Namespace, Is.EqualTo($"{Constants.PrefabNameSpacePrefix}.PrefabName"));
            Assert.That(prefabProject.AvailableVersions.Count(), Is.EqualTo(2));
            Assert.That(prefabProject.PublishedVersions.Count(), Is.EqualTo(1));
            Assert.That(prefabProject.LatestActiveVersion is not null);
            Assert.That(prefabProject.Description, Is.EqualTo("Description"));
            Assert.That(prefabProject.GroupName, Is.EqualTo("GroupName"));
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

            Assert.That(clone.ApplicationId, Is.EqualTo("ApplicationId"));
            Assert.That(clone.ProjectId, Is.Not.EqualTo("PrefabId"));
            Assert.That(clone.Name, Is.EqualTo("PrefabName")    );
            Assert.That(clone.Description, Is.EqualTo("Description"));
            Assert.That(clone.GroupName, Is.EqualTo("GroupName"));
            Assert.That(clone.PrefabRoot, Is.SameAs(prefabRoot));
            Assert.That(clone.AvailableVersions, Is.Not.SameAs(prefabDescription.AvailableVersions));
        }
    }
}
