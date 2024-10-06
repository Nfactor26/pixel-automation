using NUnit.Framework;

namespace Pixel.Automation.Editor.Notifications.Tests;

class RepositoryApplicationOpenedEventArgsFixture
{
    [Test]
    public void ValidateThatRepositoryApplicationOpenedEventArgsCanBeInitialized()
    {
        var repositoryApplicationOpenedEventArgs = new RepositoryApplicationOpenedEventArgs("AppName", "AppId");
        Assert.That(repositoryApplicationOpenedEventArgs.ApplicationName, Is.EqualTo("AppName"));
        Assert.That(repositoryApplicationOpenedEventArgs.ApplicationId, Is.EqualTo("AppId"));
    }
}
