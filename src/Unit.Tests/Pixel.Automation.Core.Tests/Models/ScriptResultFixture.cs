using NUnit.Framework;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Core.Tests.Models
{
    class ScriptResultFixture
    {
        [Test]
        public void ValidateThatScriptResultCanBeInitialized()
        {
            var scriptResult = new ScriptResult(new object(), new object());
            
            Assert.That(scriptResult.CurrentState is not null);
            Assert.That(scriptResult.ReturnValue is not null);
            Assert.That(scriptResult.IsCompleteSubmission);
        }
    }
}
