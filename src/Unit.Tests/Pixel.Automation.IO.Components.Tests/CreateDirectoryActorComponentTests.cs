using NUnit.Framework;
using Pixel.Automation.Core.Arguments;

namespace Pixel.Automation.IO.Components.Tests;

public class CreateDirectoryActorComponentTests : ActorComponentTestBase
{
    [Test]
    public async Task CreateDirectoryActorComponent_ShouldCreateDirectory()
    {
        // Arrange
        string directoryPath = Path.Combine(tempDirectory, "TestDirectory");    
        var component = new CreateDirectoryActorComponent
        {
            EntityManager = entityManager,
            Path = new InArgument<string> { DefaultValue = directoryPath },
            DirectoryInfo = new OutArgument<DirectoryInfo>()
        };

        // Act
        await component.ActAsync();

        // Assert
        Assert.That(Directory.Exists(directoryPath), Is.True);
    }
}
