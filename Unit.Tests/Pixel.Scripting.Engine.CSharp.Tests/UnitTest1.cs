using NUnit.Framework;

namespace Pixel.Scripting.Engine.CSharp.Tests
{
    public class ScriptEngineTestsPackOne
    {
        private readonly ScriptEngine scriptEngine;
        
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}