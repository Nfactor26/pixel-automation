using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;

namespace Pixel.Automation.IO.Components.Tests;

public class GetDirectoriesActorComponentTests : ActorComponentTestBase
{
    [Test]
    public async Task GetDirectoriesActorComponent_ShouldReturnDirectories()
    {
        // Arrange
        string dirPath = Path.Combine(tempDirectory, "TestDir");
        string subDir1 = Path.Combine(dirPath, "Sub1");
        string subDir2 = Path.Combine(dirPath, "Sub2");
        Directory.CreateDirectory(subDir1);
        Directory.CreateDirectory(subDir2);
        var component = new GetDirectoriesActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = dirPath },
            DirectoriesResult = new InArgument<IEnumerable<string>>()
        };

        // Act
        await component.ActAsync();

        // Assert
        await argumentProcessor.Received(1).SetValueAsync(
            Arg.Any<InArgument<IEnumerable<string>>>(),
            Arg.Is<IEnumerable<string>>(dirs => dirs.Contains(subDir1) && dirs.Contains(subDir2)));
    }

    [Test]
    public async Task GetDirectoriesActorComponent_ShouldReturnDirectories_WithSearchPattern()
    {
        // Arrange
        string dirPath = Path.Combine(tempDirectory, "TestDir");
        string subDir1 = Path.Combine(dirPath, "Alpha");
        string subDir2 = Path.Combine(dirPath, "Beta");
        Directory.CreateDirectory(subDir1);
        Directory.CreateDirectory(subDir2);
        var component = new GetDirectoriesActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = dirPath },
            SearchPattern = new InArgument<string> { DefaultValue = "A*" },
            DirectoriesResult = new InArgument<IEnumerable<string>>()
        };

        // Act
        await component.ActAsync();

        // Assert
        await argumentProcessor.Received(1).SetValueAsync(
            Arg.Any<InArgument<IEnumerable<string>>>(),
            Arg.Is<IEnumerable<string>>(dirs => dirs.Contains(subDir1) && !dirs.Contains(subDir2)));
    }

    [Test]
    public async Task GetDirectoriesActorComponent_ShouldReturnDirectories_WithSearchPatternAndEnumerationOptions()
    {
        // Arrange
        string dirPath = Path.Combine(tempDirectory, "TestDir");
        string subDir1 = Path.Combine(dirPath, "Alpha");
        string subDir2 = Path.Combine(dirPath, "Beta");
        Directory.CreateDirectory(subDir1);
        Directory.CreateDirectory(subDir2);
        var options = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };
        var component = new GetDirectoriesActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = dirPath },
            SearchPattern = new InArgument<string> { DefaultValue = "A*" },
            EnumerationOptions = new InArgument<EnumerationOptions> { DefaultValue = options },
            DirectoriesResult = new InArgument<IEnumerable<string>>()
        };
        argumentProcessor.GetValueAsync<EnumerationOptions>(Arg.Any<InArgument<EnumerationOptions>>()).Returns(options);

        // Act
        await component.ActAsync();

        // Assert
        await argumentProcessor.Received(1).SetValueAsync(
            Arg.Any<InArgument<IEnumerable<string>>>(),
            Arg.Is<IEnumerable<string>>(dirs => dirs.Contains(subDir1) && !dirs.Contains(subDir2)));
    }

    [Test]
    public void GetDirectoriesActorComponent_ShouldThrowIfDirectoryDoesNotExist()
    {
        // Arrange
        string dirPath = Path.Combine(tempDirectory, "NonExistentDir");
        var component = new GetDirectoriesActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = dirPath },
            DirectoriesResult = new InArgument<IEnumerable<string>>()
        };

        // Act & Assert
        Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await component.ActAsync());
    }
}
