using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;

namespace Pixel.Automation.IO.Components.Tests;

public class GetFilesActorComponentTests : ActorComponentTestBase
{
    [Test]
    public async Task GetFilesActorComponent_ShouldReturnFiles()
    {
        // Arrange
        string dirPath = Path.Combine(tempDirectory, "TestDir");
        Directory.CreateDirectory(dirPath);
        string file1 = Path.Combine(dirPath, "a.txt");
        string file2 = Path.Combine(dirPath, "b.txt");
        File.WriteAllText(file1, "A");
        File.WriteAllText(file2, "B");
        var component = new GetFilesActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = dirPath },
            FilesResult = new InArgument<IEnumerable<string>>()
        };

        // Act
        await component.ActAsync();

        // Assert
        await argumentProcessor.Received(1).SetValueAsync(
            Arg.Any<InArgument<IEnumerable<string>>>(),
            Arg.Is<IEnumerable<string>>(files => files.Contains(file1) && files.Contains(file2)));
    }

    [Test]
    public async Task GetFilesActorComponent_ShouldReturnFiles_WithSearchPattern()
    {
        // Arrange
        string dirPath = Path.Combine(tempDirectory, "TestDir");
        Directory.CreateDirectory(dirPath);
        string file1 = Path.Combine(dirPath, "a.txt");
        string file2 = Path.Combine(dirPath, "b.txt");
        File.WriteAllText(file1, "A");
        File.WriteAllText(file2, "B");
        var component = new GetFilesActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = dirPath },
            SearchPattern = new InArgument<string> { DefaultValue = "a.*" },
            FilesResult = new InArgument<IEnumerable<string>>()
        };

        // Act
        await component.ActAsync();

        // Assert
        await argumentProcessor.Received(1).SetValueAsync(
            Arg.Any<InArgument<IEnumerable<string>>>(),
            Arg.Is<IEnumerable<string>>(files => files.Contains(file1) && !files.Contains(file2)));
    }

    [Test]
    public async Task GetFilesActorComponent_ShouldReturnFiles_WithSearchPatternAndEnumerationOptions()
    {
        // Arrange
        string dirPath = Path.Combine(tempDirectory, "TestDir");
        Directory.CreateDirectory(dirPath);
        string file1 = Path.Combine(dirPath, "a.txt");
        string file2 = Path.Combine(dirPath, "b.txt");
        File.WriteAllText(file1, "A");
        File.WriteAllText(file2, "B");
        var options = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };
        var component = new GetFilesActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = dirPath },
            SearchPattern = new InArgument<string> { DefaultValue = "a.*" },
            EnumerationOptions = new InArgument<EnumerationOptions> { DefaultValue = options },
            FilesResult = new InArgument<IEnumerable<string>>()
        };
        argumentProcessor.GetValueAsync<EnumerationOptions>(Arg.Any<InArgument<EnumerationOptions>>()).Returns(options);

        // Act
        await component.ActAsync();

        // Assert
        await argumentProcessor.Received(1).SetValueAsync(
            Arg.Any<InArgument<IEnumerable<string>>>(),
            Arg.Is<IEnumerable<string>>(files => files.Contains(file1) && !files.Contains(file2)));
    }

    [Test]
    public void GetFilesActorComponent_ShouldThrowIfDirectoryDoesNotExist()
    {
        // Arrange
        string dirPath = Path.Combine(tempDirectory, "NonExistentDir");
        var component = new GetFilesActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = dirPath },
            FilesResult = new InArgument<IEnumerable<string>>()
        };

        // Act & Assert
        Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await component.ActAsync());
    }
}
