using NUnit.Framework;
using Pixel.Automation.Core.Arguments;

namespace Pixel.Automation.IO.Components.Tests;

public class MoveActorComponentTests : ActorComponentTestBase
{
    [Test]
    public async Task MoveActorComponent_ShouldMoveDirectory()
    {
        // Arrange
        string sourceDir = Path.Combine(tempDirectory, "SourceDir");
        string destDir = Path.Combine(tempDirectory, "DestDir");
        Directory.CreateDirectory(sourceDir);
        var component = new MoveActorComponent
        {
            EntityManager = entityManager,
            SourcePath = new InArgument<string> { DefaultValue = sourceDir },
            DestPath = new InArgument<string> { DefaultValue = destDir }
        };

        // Act
        await component.ActAsync();

        // Assert
        Assert.That(Directory.Exists(sourceDir), Is.False);
        Assert.That(Directory.Exists(destDir), Is.True);
    }

    [Test]
    public async Task MoveActorComponent_ShouldMoveFile()
    {
        // Arrange
        string sourceFile = Path.Combine(tempDirectory, "SourceFile.txt");
        string destFile = Path.Combine(tempDirectory, "DestFile.txt");
        File.WriteAllText(sourceFile, "Test file content");
        var component = new MoveActorComponent
        {
            EntityManager = entityManager,
            SourcePath = new InArgument<string> { DefaultValue = sourceFile },
            DestPath = new InArgument<string> { DefaultValue = destFile }
        };

        // Act
        await component.ActAsync();

        // Assert
        Assert.That(File.Exists(sourceFile), Is.False);
        Assert.That(File.Exists(destFile), Is.True);
        Assert.That(File.ReadAllText(destFile), Is.EqualTo("Test file content"));
    }

    [Test]
    public void MoveActorComponent_ShouldThrowIfSourceDoesNotExist()
    {
        // Arrange
        string sourceDir = Path.Combine(tempDirectory, "NonExistentSource");
        string destDir = Path.Combine(tempDirectory, "DestDir");
        var component = new MoveActorComponent
        {
            EntityManager = entityManager,
            SourcePath = new InArgument<string> { DefaultValue = sourceDir },
            DestPath = new InArgument<string> { DefaultValue = destDir }
        };

        // Act & Assert
        Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await component.ActAsync());
    }
}
