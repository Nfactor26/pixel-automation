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
            
            Assert.That(testResult.Result, Is.EqualTo(TestStatus.None));
            Assert.That(testResult.ExecutionTime, Is.EqualTo(TimeSpan.Zero));
            Assert.That(string.IsNullOrEmpty(testResult.ErrorMessage));
            Assert.That(testResult.TestData is null);
            Assert.That(testResult.Error is null);

            testResult.ExecutionTime = TimeSpan.FromSeconds(5);
            testResult.Result = TestStatus.Failed;
            testResult.Error = new Exception("Could not find control");
            testResult.TestData = "Model.ToString()";

            Assert.That(testResult.Result, Is.EqualTo(TestStatus.Failed));
            Assert.That(testResult.ExecutionTime, Is.EqualTo(TimeSpan.FromSeconds(5)));
            Assert.That(testResult.ErrorMessage, Is.EqualTo("System.Exception: Could not find control"));
            Assert.That(testResult.Error is not null);
            Assert.That(testResult.TestData, Is.EqualTo("Model.ToString()"));
        }
    }
}
