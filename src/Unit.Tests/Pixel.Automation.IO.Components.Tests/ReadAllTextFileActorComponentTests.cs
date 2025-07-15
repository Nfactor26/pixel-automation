using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Arguments;

namespace Pixel.Automation.IO.Components.Tests;

public class ReadAllTextFileActorComponentTests : ActorComponentTestBase
{
    [Test]
    public async Task ReadAllTextFileActorComponent_ShouldReadTextFromFile()
    {
        // Arrange
        string filePath = Path.Combine(tempDirectory, "TestFile.txt");
        string content = "Hello, world!";
        File.WriteAllText(filePath, content);
        var component = new ReadAllTextFileActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = filePath },
            Text = new OutArgument<string>()
        };

        // Act
        await component.ActAsync();

        // Assert
        await argumentProcessor.Received(1).SetValueAsync<string>(Arg.Any<OutArgument<string>>(), Arg.Is<string>(s => s == content));
    }

    [Test]
    public void ReadAllTextFileActorComponent_ShouldThrowIfFileDoesNotExist()
    {
        // Arrange
        string filePath = Path.Combine(tempDirectory, "NonExistentFile.txt");
        var component = new ReadAllTextFileActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = filePath },
            Text = new OutArgument<string>()
        };

        // Act & Assert
        Assert.ThrowsAsync<FileNotFoundException>(async () => await component.ActAsync());
    }
}
