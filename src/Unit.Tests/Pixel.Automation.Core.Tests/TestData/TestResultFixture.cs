using NUnit.Framework;
using Pixel.Automation.Core.TestData;
using System;

namespace Pixel.Automation.Core.Tests.TestData
{
    class TestResultFixture
    {
        [Test]
        public void ValidateThatTestResultCanBeInitialized()
        {
            var testResult = new TestResult();
            
            Assert.AreEqual(TestStatus.None, testResult.Result);
            Assert.AreEqual(TimeSpan.Zero, testResult.ExecutionTime);
            Assert.IsEmpty(testResult.ErrorMessage);
            Assert.IsNull(testResult.TestData);
            Assert.IsNull(testResult.Error);

            testResult.ExecutionTime = TimeSpan.FromSeconds(5);
            testResult.Result = TestStatus.Failed;
            testResult.Error = new Exception("Could not find control");
            testResult.TestData = "Model.ToString()";

            Assert.AreEqual(TestStatus.Failed, testResult.Result);
            Assert.AreEqual(TimeSpan.FromSeconds(5), testResult.ExecutionTime);
            Assert.AreEqual("System.Exception: Could not find control", testResult.ErrorMessage);
            Assert.IsNotNull(testResult.Error);
            Assert.AreEqual("Model.ToString()", testResult.TestData);
        }
    }
}
