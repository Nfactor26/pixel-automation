using NUnit.Framework;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.Core.Interfaces;
using System.Collections.Generic;

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
            var knownApplication = new KnownApplication("WinApplication", "Windows application",
                typeof(IApplication), new List<string>
                {
                    "WINDOWS"
                }
            );
           
            Assert.AreEqual("WinApplication", knownApplication.DisplayName);
            Assert.AreEqual("Windows application", knownApplication.Description);
            Assert.AreEqual(typeof(IApplication), knownApplication.UnderlyingApplicationType);
            Assert.IsNotEmpty(knownApplication.SupportedPlatforms);
        }
    }
}
