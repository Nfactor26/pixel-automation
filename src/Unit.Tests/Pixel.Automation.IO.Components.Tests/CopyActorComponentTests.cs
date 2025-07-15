using NUnit.Framework;
using Pixel.Automation.Core.Arguments;

namespace Pixel.Automation.IO.Components.Tests;

public class CopyActorComponentTests : ActorComponentTestBase
{
    [Test]
    public async Task ValidateCopyActorCanCopyFile()
    {
        // Arrange
        File.WriteAllText(Path.Combine(tempDirectory, "TestFile.txt"), "Test Content");
        var component = new CopyActorComponent
        {
            EntityManager = entityManager,
            SourcePath = new InArgument<string> { DefaultValue = Path.Combine(tempDirectory, "TestFile.txt") },
            TargetPath = new InArgument<string> { DefaultValue = Path.Combine(tempDirectory, "CopiedFile.txt") },
            Overwrite = new InArgument<bool> { DefaultValue = false }
        };

        // Act
        await component.ActAsync();

        // Assert
        Assert.That(File.Exists(Path.Combine(tempDirectory, "CopiedFile.txt")), "File was not copied");
        Assert.That(File.ReadAllText(Path.Combine(tempDirectory, "CopiedFile.txt")), Is.EqualTo("Test Content"), "File contents don't match");
    }

    [Test]
    public async Task ValidateCopyActorCanCopyFileWithOverwrite()
    {
        // Arrange
        File.WriteAllText(Path.Combine(tempDirectory, "TestFile.txt"), "Test Content");
        File.WriteAllText(Path.Combine(tempDirectory, "CopiedFile.txt"), "Copy Content");
        var component = new CopyActorComponent
        {
            EntityManager = entityManager,
            SourcePath = new InArgument<string> { DefaultValue = Path.Combine(tempDirectory, "TestFile.txt") },
            TargetPath = new InArgument<string> { DefaultValue = Path.Combine(tempDirectory, "CopiedFile.txt") },
            Overwrite = new InArgument<bool> { DefaultValue = true }
        };

        // Act
        await component.ActAsync();

        // Assert
        Assert.That(File.Exists(Path.Combine(tempDirectory, "CopiedFile.txt")), "File was not copied");
        Assert.That(File.ReadAllText(Path.Combine(tempDirectory, "CopiedFile.txt")), Is.EqualTo("Test Content"), "File contents don't match");
    }

    [Test]
    public async Task ValidateCopyActorCanCopyDirectoryRecursively()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(tempDirectory, "Source"));
        File.WriteAllText(Path.Combine(tempDirectory, "Source", "File1.txt"), "Content1");
        Directory.CreateDirectory(Path.Combine(tempDirectory, "Source", "SubDir"));
        File.WriteAllText(Path.Combine(tempDirectory, "Source", "SubDir", "File2.txt"), "Content2");

        var component = new CopyActorComponent
        {
            EntityManager = entityManager,
            SourcePath = new InArgument<string> { DefaultValue = Path.Combine(tempDirectory, "Source") },
            TargetPath = new InArgument<string> { DefaultValue = Path.Combine(tempDirectory, "Target") },
            Recursive = new InArgument<bool> { DefaultValue = true }
        };

        // Act
        await component.ActAsync();

        // Assert
        Assert.That(Directory.Exists(Path.Combine(tempDirectory, "Target")), Is.True);
        Assert.That(File.Exists(Path.Combine(tempDirectory, "Target", "File1.txt")), Is.True);
        Assert.That(File.Exists(Path.Combine(tempDirectory, "Source", "SubDir", "File2.txt")), Is.True);
    }   
  
}