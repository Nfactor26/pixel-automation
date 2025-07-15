using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.IO.Components.Tests;

public  class ExistsActorComponentTests : ActorComponentTestBase
{
    [Test]
    public async Task ValideExistShouldReturnTrueWhenFileExists()
    {
        // Arrange
        string filePath = Path.Combine(tempDirectory, "TestFile.txt");
        File.WriteAllText(filePath, "Test Content");
        var component = new ExistsActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = filePath }
        };

        // Act
        await component.ActAsync();

        // Assert
        await argumentProcessor.Received(1).SetValueAsync<bool>(Arg.Any<OutArgument<bool>>(), Arg.Is<bool>(true));
    }

    [Test]
    public async Task ValideExistShouldReturnTrueWhenFolderExists()
    {
        // Arrange     
        string folderPath = Path.Combine(tempDirectory, "Data");
        Directory.CreateDirectory(folderPath);
        var component = new ExistsActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = folderPath }
        };

        // Act
        await component.ActAsync();

        // Assert
        await argumentProcessor.Received(1).SetValueAsync<bool>(Arg.Any<OutArgument<bool>>(), Arg.Is<bool>(true));
    }

    [Test]
    public async Task ValidateExistsShouldReturnFalseWhenFileDoesNotExist()
    {
        string filePath = Path.Combine(tempDirectory, "NonExistentFile.txt");
        // Arrange
        var component = new ExistsActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = filePath }
        };

        // Act
        await component.ActAsync();

        // Assert
        await argumentProcessor.Received(1).SetValueAsync<bool>(Arg.Any<OutArgument<bool>>(), Arg.Is<bool>(false));
    }

    [Test]
    public async Task ValidateExistsShouldReturnFalseWhenFolderDoesNotExist()
    {
        // Arrange
        string folderPath = Path.Combine(tempDirectory, "NonExistentFolder");
        var component = new ExistsActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = folderPath }
        };

        // Act
        await component.ActAsync();

        // Assert
        await argumentProcessor.Received(1).SetValueAsync<bool>(Arg.Any<OutArgument<bool>>(), Arg.Is<bool>(false));
    }
}
