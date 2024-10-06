using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Components.Decisions;
using Pixel.Automation.Core.Interfaces;
using System;


namespace Pixel.Automation.Core.Components.Tests
{
    public class ScriptFileInitializerTest
    {
        [Test]
        public void ValidateThatScriptFileInitializerCanSetupScriptPathCorrectlyForComponents()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.WorkingDirectory.Returns(Environment.CurrentDirectory);
            fileSystem.ScriptsDirectory.Returns(Environment.CurrentDirectory);
            entityManager.GetCurrentFileSystem().Returns(fileSystem);

            var scriptableComponent = new IfEntity() { EntityManager = entityManager };
            Assert.That(scriptableComponent.ScriptFile is null);

            var scriptFileInitializer = new ScriptFileInitializer();
            scriptFileInitializer.IntializeComponent(scriptableComponent, entityManager);

            Assert.That(scriptableComponent.ScriptFile is not null);
        }
    }
}
