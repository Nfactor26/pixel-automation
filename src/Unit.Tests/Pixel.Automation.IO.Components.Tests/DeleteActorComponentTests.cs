using NUnit.Framework;
using Pixel.Automation.Core.Arguments;

namespace Pixel.Automation.IO.Components.Tests;

public class DeleteActorComponentTests : ActorComponentTestBase
{
    [Test]
    public async Task DeleteActorComponent_ShouldDeleteFile()
    {
        // Arrange
        string filePath = Path.Combine(tempDirectory, "TestFile.txt");
        File.WriteAllText(filePath, "Test");
        var component = new DeleteActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = filePath },
            Recursive = new InArgument<bool> { DefaultValue = false }
        };

        // Act
        await component.ActAsync();

        // Assert
        Assert.That(File.Exists(filePath), Is.False);
    }

    [Test]
    public async Task DeleteActorComponent_ShouldDeleteDirectory()
    {
        // Arrange
        string dirPath = Path.Combine(tempDirectory, "TestDir");
        Directory.CreateDirectory(dirPath);
        var component = new DeleteActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = dirPath },
            Recursive = new InArgument<bool> { DefaultValue = true }
        };

        // Act
        await component.ActAsync();

        // Assert
        Assert.That(Directory.Exists(dirPath), Is.False);
    }

    [Test]
    public void DeleteActorComponent_ShouldThrowIfFileDoesNotExist()
    {
        // Arrange
        string filePath = Path.Combine(tempDirectory, "NonExistentFile.txt");
        var component = new DeleteActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = filePath },
            Recursive = new InArgument<bool> { DefaultValue = false }
        };

        // Act & Assert
        Assert.ThrowsAsync<FileNotFoundException>(async () => await component.ActAsync());
    }

    [Test]
    public void DeleteActorComponent_ShouldThrowIfDirectoryDoesNotExist()
    {
        // Arrange
        string dirPath = Path.Combine(tempDirectory, "NonExistentDir");
        var component = new DeleteActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = dirPath },
            Recursive = new InArgument<bool> { DefaultValue = true }
        };

        // Act & Assert
        Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await component.ActAsync());
    }
    
}
