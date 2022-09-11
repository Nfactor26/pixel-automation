using NUnit.Framework;

namespace Pixel.Automation.Editor.Notifications.Tests;

class RepositoryApplicationOpenedEventArgsFixture
{
    [Test]
    public void ValidateThatRepositoryApplicationOpenedEventArgsCanBeInitialized()
    {
        var repositoryApplicationOpenedEventArgs = new RepositoryApplicationOpenedEventArgs("AppName", "AppId");
        Assert.AreEqual("AppName", repositoryApplicationOpenedEventArgs.ApplicationName);
        Assert.IsNotNull("AppId", repositoryApplicationOpenedEventArgs.ApplicationId);
    }
}
