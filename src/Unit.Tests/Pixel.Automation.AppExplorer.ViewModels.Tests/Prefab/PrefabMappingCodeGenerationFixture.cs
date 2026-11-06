using NUnit.Framework;
using Pixel.Automation.AppExplorer.ViewModels.PrefabDropHandler;
using Pixel.Automation.Editor.Core;
using System;

namespace Pixel.Automation.AppExplorer.ViewModels.Tests.Prefab
{
    [TestFixture]
    public class PrefabMappingCodeGenerationFixture
    {
        [Test]
        public void InputMappingMapsSimpleTypesDirectly()
        {
            var code = new PrefabInputMapper().GeneratedMappingCode(
                new[] { Mapping("Name", typeof(string), "Name", typeof(string)) },
                typeof(SourceModel), typeof(PrefabDataModel));

            Assert.That(code, Does.Contain("prefabModel.Name = Name;"));
            Assert.That(code, Does.Not.Contain("AutoMapper"));
            Assert.That(code, Does.Not.Contain("TODO"));
        }

        [Test]
        public void InputMappingLeavesManualTodoForNestedTypes()
        {
            var code = new PrefabInputMapper().GeneratedMappingCode(
                new[] { Mapping("Address", typeof(Address), "Address", typeof(Address)) },
                typeof(SourceModel), typeof(PrefabDataModel));

            Assert.That(code, Does.Contain("TODO: Manually map Address (Address) to prefabModel.Address (Address)."));
            Assert.That(code, Does.Not.Contain("prefabModel.Address = Address;"));
        }

        [Test]
        public void OutputMappingMapsSimpleTypesDirectly()
        {
            var code = new PrefabOutputMapper().GeneratedMappingCode(
                new[] { Mapping("Name", typeof(string), "Name", typeof(string)) },
                typeof(PrefabDataModel), typeof(SourceModel));

            Assert.That(code, Does.Contain("Name = prefabModel.Name;"));
            Assert.That(code, Does.Not.Contain("AutoMapper"));
            Assert.That(code, Does.Not.Contain("TODO"));
        }

        [Test]
        public void OutputMappingLeavesManualTodoForNestedTypes()
        {
            var code = new PrefabOutputMapper().GeneratedMappingCode(
                new[] { Mapping("Address", typeof(Address), "Address", typeof(Address)) },
                typeof(PrefabDataModel), typeof(SourceModel));

            Assert.That(code, Does.Contain("TODO: Manually map prefabModel.Address (Address) to Address (Address)."));
            Assert.That(code, Does.Not.Contain("Address = prefabModel.Address;"));
        }

        private static PropertyMap Mapping(string sourceName, Type sourceType, string destinationName, Type destinationType)
        {
            return new PropertyMap
            {
                AssignFrom = sourceName,
                AssignFromType = sourceType,
                AssignTo = destinationName,
                AssignToType = destinationType
            };
        }

        private class SourceModel { }
        private class PrefabDataModel { }
        private class Address { public string City { get; set; } }
    }
}
