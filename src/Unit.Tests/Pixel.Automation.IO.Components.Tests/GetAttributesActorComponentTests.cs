using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;

namespace Pixel.Automation.IO.Components.Tests;

public class GetAttributesActorComponentTests : ActorComponentTestBase
{
    [Test]
    public async Task GetAttributesActorComponent_ShouldReturnFileAttributes()
    {
        // Arrange
        string filePath = Path.Combine(tempDirectory, "TestFile.txt");
        File.WriteAllText(filePath, "Test");
        File.SetAttributes(filePath, FileAttributes.Hidden);
        var component = new GetAttributesActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = filePath },
            FileAttributes = new OutArgument<FileAttributes>()
        };

        // Act
        await component.ActAsync();

        // Assert
        await argumentProcessor.Received(1).SetValueAsync<FileAttributes>(Arg.Any<OutArgument<FileAttributes>>(), FileAttributes.Hidden);
    }

    [Test]
    public void GetAttributesActorComponent_ShouldThrowIfFileDoesNotExist()
    {
        // Arrange
        string filePath = Path.Combine(tempDirectory, "NonExistentFile.txt");
        var component = new GetAttributesActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = filePath },
            FileAttributes = new OutArgument<FileAttributes>()
        };

        // Act & Assert
        Assert.ThrowsAsync<FileNotFoundException>(async () => await component.ActAsync());
    }
}
