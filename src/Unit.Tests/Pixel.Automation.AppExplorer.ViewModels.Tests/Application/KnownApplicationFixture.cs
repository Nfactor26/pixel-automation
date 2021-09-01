using NUnit.Framework;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.AppExplorer.ViewModels.Tests
{
    /// <summary>
    /// Test Fixture for <see cref="KnownApplication"/>
    /// </summary>
    [TestFixture]
    public class KnownApplicationFixture
    {
        [Test]
        public void ValidateThatKnownApplicationCanBeCorrectlyInitialized()
        {
            var knownApplication = new KnownApplication("WinApplication", "Windows application", typeof(IApplication));
           
            Assert.AreEqual("WinApplication", knownApplication.DisplayName);
            Assert.AreEqual("Windows application", knownApplication.Description);
            Assert.AreEqual(typeof(IApplication), knownApplication.UnderlyingApplicationType);
        }
    }
}
