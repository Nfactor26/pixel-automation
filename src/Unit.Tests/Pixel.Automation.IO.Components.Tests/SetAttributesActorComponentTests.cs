using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;

namespace Pixel.Automation.IO.Components.Tests;

public class SetAttributesActorComponentTests : ActorComponentTestBase
{
    [Test]
    public async Task SetAttributesActorComponent_ShouldSetFileAttributes()
    {
        // Arrange
        string filePath = Path.Combine(tempDirectory, "TestFile.txt");
        File.WriteAllText(filePath, "Test");
        var component = new SetAttributesActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = filePath },
            FileAttributes = new OutArgument<FileAttributes>()
        };

        argumentProcessor.GetValueAsync<FileAttributes>(Arg.Any<OutArgument<FileAttributes>>())
            .Returns(FileAttributes.ReadOnly);

        // Act
        await component.ActAsync();

        // Assert
        Assert.That((File.GetAttributes(filePath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
    }

    [Test]
    public void SetAttributesActorComponent_ShouldThrowIfFileDoesNotExist()
    {
        // Arrange
        string filePath = Path.Combine(tempDirectory, "NonExistentFile.txt");
        var component = new SetAttributesActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = filePath },
            FileAttributes = new OutArgument<FileAttributes>()
        };
        argumentProcessor.GetValueAsync<FileAttributes>(Arg.Any<OutArgument<FileAttributes>>())
            .Returns(FileAttributes.ReadOnly);

        // Act & Assert
        Assert.ThrowsAsync<FileNotFoundException>(async () => await component.ActAsync());
    }

}
