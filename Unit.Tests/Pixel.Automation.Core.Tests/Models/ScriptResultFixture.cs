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
            
            Assert.IsNotNull(scriptResult.CurrentState);
            Assert.IsNotNull(scriptResult.ReturnValue);
            Assert.IsTrue(scriptResult.IsCompleteSubmission);
        }
    }
}
