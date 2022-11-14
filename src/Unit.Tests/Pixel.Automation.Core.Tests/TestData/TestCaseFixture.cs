using NUnit.Framework;
using Pixel.Automation.Core.TestData;
using System;

namespace Pixel.Automation.Core.Tests.TestData
{
    class TestCaseFixture
    {
        [Test]
        public void ValidateThatTestCaseCanBeInitialized()
        {
            var testCase = new TestCase();

            Assert.IsFalse(string.IsNullOrEmpty(testCase.TestCaseId));
            Assert.IsNull(testCase.FixtureId);
            Assert.IsNull(testCase.DisplayName);
            Assert.AreEqual(0, testCase.Order);
            Assert.IsFalse(testCase.IsMuted);
            Assert.IsNull(testCase.ScriptFile);
            Assert.IsNull(testCase.TestDataId);
            Assert.IsNull(testCase.Description);
            Assert.IsNotNull(testCase.Tags);
            Assert.IsNull(testCase.TestCaseEntity);
            Assert.AreEqual(testCase.TestCaseId, testCase.ToString());

            testCase.FixtureId = Guid.NewGuid().ToString();
            testCase.DisplayName = "TestCase";
            testCase.Order = 10;
            testCase.IsMuted = true;
            testCase.ScriptFile = "TestCase.csx";
            testCase.TestDataId = Guid.NewGuid().ToString();
            testCase.Tags.Add("color", "red");
            testCase.Description = "Description";
            testCase.TestCaseEntity = new Entity();

            Assert.IsFalse(string.IsNullOrEmpty(testCase.FixtureId));
            Assert.AreEqual("TestCase", testCase.DisplayName);
            Assert.AreEqual(10, testCase.Order);
            Assert.IsTrue(testCase.IsMuted);
            Assert.AreEqual("TestCase.csx", testCase.ScriptFile);
            Assert.IsFalse(string.IsNullOrEmpty(testCase.TestDataId));
            Assert.AreEqual(testCase.Tags["color"], "red");
            Assert.AreEqual("Description", testCase.Description);
            Assert.IsNotNull(testCase.TestCaseEntity);
        }

        [Test]
        public void ValidateThatTestCaseCanBeCloned()
        {
            var testCase = new TestCase() { DisplayName = "TestCase", FixtureId = "FixtureId", Order = 10, IsMuted = true, ScriptFile = "ScriptFile.csx", TestDataId = "TestDataId", Description = "Description", TestCaseEntity = new Entity() };
            testCase.Tags.Add("color", "red");
            var copyOfTestCase = testCase.Clone() as TestCase;

            Assert.AreEqual(testCase.DisplayName, copyOfTestCase.DisplayName);
            Assert.AreEqual(testCase.Order, copyOfTestCase.Order);
            Assert.AreEqual(testCase.Description, copyOfTestCase.Description);
            Assert.AreEqual(testCase.IsMuted, copyOfTestCase.IsMuted);
            Assert.AreEqual(copyOfTestCase.Tags["color"], "red");
            Assert.AreNotEqual(testCase.FixtureId, copyOfTestCase.FixtureId);
            Assert.AreNotEqual(testCase.ScriptFile, copyOfTestCase.ScriptFile);
            Assert.AreNotEqual(testCase.TestDataId, copyOfTestCase.TestDataId);
            Assert.AreNotEqual(testCase.TestCaseEntity, copyOfTestCase.TestCaseEntity);

        }
    }
}
