using NUnit.Framework;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

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
           
            Assert.That(knownApplication.DisplayName, Is.EqualTo("WinApplication"));
            Assert.That(knownApplication.Description, Is.EqualTo("Windows application"));
            Assert.That(knownApplication.UnderlyingApplicationType, Is.EqualTo(typeof(IApplication)));
            Assert.That(knownApplication.SupportedPlatforms.Any());
        }
    }
}
