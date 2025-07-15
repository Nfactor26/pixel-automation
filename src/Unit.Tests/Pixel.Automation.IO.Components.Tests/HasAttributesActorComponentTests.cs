using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;

namespace Pixel.Automation.IO.Components.Tests;

public class HasAttributesActorComponentTests : ActorComponentTestBase
{
    [Test]
    public async Task HasAttributesActorComponent_ShouldReturnTrueIfFileHasAttribute()
    {
        // Arrange
        string filePath = Path.Combine(tempDirectory, "TestFileHidden.txt");
        File.WriteAllText(filePath, "Test");
        File.SetAttributes(filePath, FileAttributes.Hidden);
        var component = new HasAttributesActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = filePath },
            AttributeToCheck = new InArgument<FileAttributes> { DefaultValue = FileAttributes.Hidden },
            HasAttribute = new OutArgument<bool>()
        };

        // Act
        await component.ActAsync();

        // Assert
        await argumentProcessor.Received(1).SetValueAsync<bool>(Arg.Any<OutArgument<bool>>(), true);
    }

    [Test]
    public async Task HasAttributesActorComponent_ShouldReturnFalseIfFileDoesNotHaveAttribute()
    {
        // Arrange
        string filePath = Path.Combine(tempDirectory, "TestFileNormal.txt");
        File.WriteAllText(filePath, "Test");
        File.SetAttributes(filePath, FileAttributes.Normal);
        var component = new HasAttributesActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = filePath },
            AttributeToCheck = new InArgument<FileAttributes> { DefaultValue = FileAttributes.Hidden },
            HasAttribute = new OutArgument<bool>()
        };

        // Act
        await component.ActAsync();

        // Assert
        await argumentProcessor.Received(1).SetValueAsync<bool>(Arg.Any<OutArgument<bool>>(), false);
    }

    [Test]
    public void HasAttributesActorComponent_ShouldThrowIfFileDoesNotExist()
    {
        // Arrange
        string filePath = Path.Combine(tempDirectory, "NonExistentFile.txt");
        var component = new HasAttributesActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = filePath },
            AttributeToCheck = new InArgument<FileAttributes> { DefaultValue = FileAttributes.Hidden },
            HasAttribute = new OutArgument<bool>()
        };

        // Act & Assert
        Assert.ThrowsAsync<FileNotFoundException>(async () => await component.ActAsync());
    }
}
