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

            Assert.That(string.IsNullOrEmpty(testCase.TestCaseId) == false);
            Assert.That(testCase.FixtureId is null);
            Assert.That(testCase.DisplayName is null);
            Assert.That(0, Is.EqualTo(testCase.Order));
            Assert.That(testCase.IsMuted == false);
            Assert.That(testCase.ScriptFile is null);
            Assert.That(testCase.TestDataId is null);
            Assert.That(testCase.Description is null);
            Assert.That(testCase.Tags is not null);
            Assert.That(testCase.TestCaseEntity is null);
            Assert.That(testCase.TestCaseId, Is.EqualTo(testCase.ToString()));

            testCase.FixtureId = Guid.NewGuid().ToString();
            testCase.DisplayName = "TestCase";
            testCase.Order = 10;
            testCase.IsMuted = true;
            testCase.ScriptFile = "TestCase.csx";
            testCase.TestDataId = Guid.NewGuid().ToString();
            testCase.Tags.Add("color", "red");
            testCase.Description = "Description";
            testCase.TestCaseEntity = new Entity();

            Assert.That(string.IsNullOrEmpty(testCase.FixtureId) == false);
            Assert.That("TestCase", Is.EqualTo(testCase.DisplayName));
            Assert.That(10, Is.EqualTo(testCase.Order));
            Assert.That(testCase.IsMuted);
            Assert.That("TestCase.csx", Is.EqualTo(testCase.ScriptFile));
            Assert.That(string.IsNullOrEmpty(testCase.TestDataId) == false);
            Assert.That(testCase.Tags["color"], Is.EqualTo("red"));
            Assert.That("Description", Is.EqualTo(testCase.Description));
            Assert.That(testCase.TestCaseEntity is not null);
        }

        [Test]
        public void ValidateThatTestCaseCanBeCloned()
        {
            var testCase = new TestCase() { DisplayName = "TestCase", FixtureId = "FixtureId", Order = 10, IsMuted = true, ScriptFile = "ScriptFile.csx", TestDataId = "TestDataId", Description = "Description", TestCaseEntity = new Entity() };
            testCase.Tags.Add("color", "red");
            var copyOfTestCase = testCase.Clone() as TestCase;

            Assert.That(testCase.DisplayName, Is.EqualTo(copyOfTestCase.DisplayName));
            Assert.That(testCase.Order, Is.EqualTo(copyOfTestCase.Order));
            Assert.That(testCase.Description, Is.EqualTo(copyOfTestCase.Description));
            Assert.That(testCase.IsMuted, Is.EqualTo(copyOfTestCase.IsMuted));
            Assert.That(copyOfTestCase.Tags["color"], Is.EqualTo("red"));
            Assert.That(testCase.FixtureId, Is.Not.EqualTo(copyOfTestCase.FixtureId));
            Assert.That(testCase.ScriptFile, Is.Not.EqualTo(copyOfTestCase.ScriptFile));
            Assert.That(testCase.TestDataId, Is.Not.EqualTo(copyOfTestCase.TestDataId));
            Assert.That(testCase.TestCaseEntity, Is.Not.EqualTo(copyOfTestCase.TestCaseEntity));

        }
    }
}
