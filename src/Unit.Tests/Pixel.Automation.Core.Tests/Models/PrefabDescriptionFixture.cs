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
            var prefabDescription = new PrefabDescription(prefabRoot);

            Assert.AreEqual(null, prefabDescription.ApplicationId);
            Assert.AreEqual(null, prefabDescription.PrefabId);
            Assert.AreEqual(null, prefabDescription.PrefabName);
            Assert.AreEqual(null, prefabDescription.NameSpace);
            Assert.NotNull(prefabDescription.AvailableVersions);
            Assert.NotNull(prefabDescription.DeployedVersions);
            Assert.IsNull(prefabDescription.ActiveVersion);
            Assert.AreEqual(null, prefabDescription.Description);
            Assert.AreEqual("Default", prefabDescription.GroupName);


            prefabDescription.ApplicationId = "ApplicationId";
            prefabDescription.PrefabId = "PrefabId";
            prefabDescription.PrefabName = "PrefabName";
            prefabDescription.NameSpace = "Prefab.PrefabName";
            prefabDescription.AvailableVersions.Add(new PrefabVersion() { IsDeployed = true, Version = new Version(1, 0) });
            prefabDescription.AvailableVersions.Add(new PrefabVersion() { IsActive = true, Version = new Version(2, 0) });
            prefabDescription.Description = "Description";
            prefabDescription.GroupName = "GroupName";


            Assert.AreEqual("ApplicationId", prefabDescription.ApplicationId);
            Assert.AreEqual("PrefabId", prefabDescription.PrefabId);
            Assert.AreEqual("PrefabName", prefabDescription.PrefabName);
            Assert.AreEqual("Prefab.PrefabName", prefabDescription.NameSpace);
            Assert.AreEqual(2, prefabDescription.AvailableVersions.Count());
            Assert.AreEqual(1, prefabDescription.DeployedVersions.Count());
            Assert.NotNull(prefabDescription.ActiveVersion);
            Assert.AreEqual("Description", prefabDescription.Description);
            Assert.AreEqual("GroupName", prefabDescription.GroupName);
        }

        [Test]
        public void ValdiateThatPrefabDescriptionCanBeCorrectlyCloned()
        {
            var prefabRoot = Substitute.For<IComponent, ICloneable>();
            (prefabRoot as ICloneable).Clone().Returns(prefabRoot);

            var prefabDescription = new PrefabDescription(prefabRoot)
            {
                ApplicationId = "ApplicationId",
                PrefabId = "PrefabId",
                PrefabName = "PrefabName",
                Description = "Description",
                GroupName = "GroupName",
            };

            var clone = prefabDescription .Clone() as PrefabDescription;

            Assert.AreEqual("ApplicationId", clone.ApplicationId);
            Assert.AreNotEqual("PrefabId", clone.PrefabId);
            Assert.AreEqual("PrefabName", clone.PrefabName);
            Assert.AreEqual("Description", clone.Description);
            Assert.AreEqual("GroupName", clone.GroupName);
            Assert.AreSame(prefabRoot, clone.PrefabRoot);
            Assert.AreNotSame(prefabDescription.AvailableVersions, clone.AvailableVersions);
        }
    }
}
