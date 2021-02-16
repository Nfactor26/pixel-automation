using NUnit.Framework;
using Pixel.Automation.Core.Models;
using System.Linq;

namespace Pixel.Automation.Core.Tests.Models
{
    class AssemblyReferencesFixture
    {
        [Test]
        public void ValidateThatDefaultReferencesCanBeRetrieved()
        {
            var assemblyReferences = new AssemblyReferences();         
            var defaultReferences = assemblyReferences.GetReferencesOrDefault();
            Assert.AreEqual(8, defaultReferences.Count());
        }

        [Test]
        public void ValidatethatReferencesCanBeAdded()
        {
            var assemblyReferences = new AssemblyReferences();          
            assemblyReferences.AddReferences(new[] { typeof(AssemblyReferencesFixture).Assembly.GetName().Name });

            Assert.AreEqual(1, assemblyReferences.GetReferencesOrDefault().Count());
        }
    }
}
