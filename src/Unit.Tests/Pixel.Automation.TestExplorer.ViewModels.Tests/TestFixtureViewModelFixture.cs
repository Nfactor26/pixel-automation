using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.TestData;

namespace Pixel.Automation.TestExplorer.ViewModels.Tests
{
    public class TestFixtureViewModelFixture
    {
        /// <summary>
        /// Initialize a TestFixtureViewModel from a TestFixture object and verify that TestFixtureViewModel properties returns correct values
        /// </summary>
        [Test]
        public void ValidateThatTestCaseViewModelReturnsCorrectValues()
        {
            var fixtureEntity = new Entity();          
            TestFixture testFixture = new TestFixture()
            {
                DisplayName = "TestFixture#1",
                Order = 1,
                Description = "Test fixture description",                          
                ScriptFile = "Script.csx",
                TestFixtureEntity = fixtureEntity
            };
            testFixture.Tags.Add("color", "red");
            testFixture.Tags.Add("priority", "low");
            TestFixtureViewModel testFixtureviewModel = new TestFixtureViewModel(testFixture);

            Assert.IsTrue(!string.IsNullOrEmpty(testFixtureviewModel.Id));
            Assert.AreEqual("TestFixture#1", testFixtureviewModel.DisplayName);
            Assert.AreEqual(1, testFixtureviewModel.Order);
            Assert.AreEqual("Test fixture description", testFixtureviewModel.Description);          
            Assert.IsTrue(testFixtureviewModel.Tags.Contains("color"));
            Assert.IsTrue(testFixtureviewModel.Tags.Contains("priority"));
            Assert.AreEqual("Script.csx", testFixtureviewModel.ScriptFile);
            Assert.AreEqual(fixtureEntity, testFixtureviewModel.TestFixtureEntity);
            Assert.IsFalse(testFixtureviewModel.IsMuted);
            Assert.IsFalse(testFixtureviewModel.IsSelected);
            Assert.IsTrue(testFixtureviewModel.IsVisible);
            Assert.IsFalse(testFixtureviewModel.IsOpenForEdit);
            Assert.IsTrue(testFixtureviewModel.CanOpenForEdit);
            Assert.IsNotNull(testFixtureviewModel.Tests);
        }

        /// <summary>
        /// Initialize a TestCaseViewModel from a TestCase object and verify that TestCaseViewModel properties returns correct values
        /// </summary>
        [Test]
        public void ValidateThatTestFixtureValuesCanBeUpdatedByUpdatingWrapperPropertiesOnViewModel()
        {
            var fixtureEntity = new Entity();          
            TestFixture testFixture = new TestFixture();
            var testCaseViewModel = new TestFixtureViewModel(testFixture)
            {
                DisplayName = "TestFixture#1",
                Order = 1,
                Description = "Test fixture description",
                TestFixtureEntity = fixtureEntity,
                ScriptFile = "Script.csx",
                IsMuted = true
            };
            testCaseViewModel.Tags.Add("color", "red");
            testCaseViewModel.Tags.Add("priority", "high");

            Assert.AreEqual("TestFixture#1", testFixture.DisplayName);
            Assert.AreEqual(1, testFixture.Order);
            Assert.AreEqual("Test fixture description", testFixture.Description);
            Assert.AreEqual(fixtureEntity, testFixture.TestFixtureEntity);
            Assert.IsTrue(testCaseViewModel.Tags.Contains("color"));
            Assert.IsTrue(testCaseViewModel.Tags.Contains("priority"));
            Assert.AreEqual("Script.csx", testFixture.ScriptFile);
            Assert.IsTrue(testFixture.IsMuted);
        }

        /// <summary>
        /// Validate that visibility property is properly updated based on filter text applied
        /// </summary>
        /// <param name="filterText"></param>
        /// <param name="shouldBeVisible"></param>
        [TestCase("#1", true)]
        [TestCase("Test", true)]
        [TestCase("Fixture", true)]
        [TestCase("", true)]
        [TestCase("#2", false)]
        public void ValidateThatVisibilityIsCorrectlyUpdatedWhenFilterTextChanges(string filterText, bool shouldBeVisible)
        {
            TestFixture testFixture = new TestFixture()
            {
                DisplayName = "TestFixture#1"
            };
            TestFixtureViewModel testCaseViewModel = new TestFixtureViewModel(testFixture);
            testCaseViewModel.UpdateVisibility(filterText);

            Assert.AreEqual(shouldBeVisible, testCaseViewModel.IsVisible);
        }

        /// <summary>
        /// Validate that TestFixture should be visible when any of the test case belonging to this fixture is visible even if TestFixture itself doesn't match filter text
        /// </summary>
        /// <param name="filterText"></param>
        /// <param name="shouldBeVisible"></param>
        [TestCase("#1", true)]
        [TestCase("Test", true)]
        [TestCase("Fixture", true)]
        [TestCase("TestFixture#1", true)]
        [TestCase("TestCase#1", true)]
        [TestCase("", true)]
        [TestCase("#2", false)]
        [TestCase("name:", true)] 
        [TestCase("name:#1:Fixture", true)] 
        [TestCase("name:#1", true)] 
        [TestCase("module:test", false)] 
        [TestCase("module:test explorer", true)]
        public void ValidateThatTestFixtureIsVisibleIfAnyOfItsTestCaseIsVisible(string filterText, bool shouldBeVisible)
        {
            var testCase = new TestCase() { DisplayName = "TestCase#1" };
            var testCaseViewModel = new TestCaseViewModel(testCase);
            TestFixture testFixture = new TestFixture()
            {
                DisplayName = "TestFixture#1"
            };
            TestFixtureViewModel testfixtureViewModel = new TestFixtureViewModel(testFixture);
            testfixtureViewModel.Tags.Add("module", "test explorer");
            testfixtureViewModel.Tests.Add(testCaseViewModel);
            testfixtureViewModel.UpdateVisibility(filterText);

            Assert.AreEqual(shouldBeVisible, testfixtureViewModel.IsVisible);
        }
    }
}
