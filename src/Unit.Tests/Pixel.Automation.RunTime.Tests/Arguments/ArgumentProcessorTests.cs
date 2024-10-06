using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;

namespace Pixel.Automation.RunTime.Tests
{
    public class ArgumentProcessorTests
    {
        /// <summary>
        /// Validate that ArgumentProcessor can be constructed
        /// </summary>
        [Test]
        public void CanBeInitialized()
        {
            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            var argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(scriptEngine, new object());

            Assert.That(argumentProcessor is not null);
        }     
    }
}
