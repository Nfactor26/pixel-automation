using NUnit.Framework;
using Pixel.Automation.Core.Args;

namespace Pixel.Automation.Core.Tests.Args
{
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
}
