using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;

namespace Pixel.Automation.TestExplorer.ViewModels.Tests
{
    /// <summary>
    /// Test Fixture for <see cref="TestFixtureViewModel"/>
    /// </summary>
    [TestFixture]
    public class TestFixtureViewModelFixture
    {
        /// <summary>
        /// Initialize a TestFixtureViewModel from a TestFixture object and verify the initial state 
        /// </summary>
        [Test]
        public void ValidateThatTestCaseViewModelIsCorrectlyInitialized()
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

            Assert.That(testFixtureviewModel.TestFixture is not null);
            Assert.That(!string.IsNullOrEmpty(testFixtureviewModel.FixtureId));
            Assert.That(testFixtureviewModel.DisplayName, Is.EqualTo("TestFixture#1"));
            Assert.That(testFixtureviewModel.Order, Is.EqualTo(1));
            Assert.That(testFixtureviewModel.Description, Is.EqualTo("Test fixture description"));          
            Assert.That(testFixtureviewModel.Tags.Contains("color"));
            Assert.That(testFixtureviewModel.Tags.Contains("priority"));
            Assert.That(testFixtureviewModel.ScriptFile, Is.EqualTo("Script.csx"));
            Assert.That(testFixtureviewModel.TestFixtureEntity, Is.EqualTo(fixtureEntity));
            Assert.That(testFixtureviewModel.IsMuted == false);
            Assert.That(testFixtureviewModel.IsSelected == false);
            Assert.That(testFixtureviewModel.IsVisible);
            Assert.That(testFixtureviewModel.IsOpenForEdit == false);
            Assert.That(testFixtureviewModel.CanOpenForEdit);
            Assert.That(testFixtureviewModel.Tests is not null);
        }

        /// <summary>
        /// Initialize a TestCaseViewModel from a TestCase object and verify that TestCaseViewModel properties returns correct values
        /// </summary>
        [Test]
        public void ValidateThatTestFixtureValuesCanBeUpdatedByUpdatingWrapperPropertiesOnViewModel()
        {
            var fixtureEntity = new Entity();          
            TestFixture testFixture = new TestFixture();
            var testFixtureViewModel = new TestFixtureViewModel(testFixture)
            {
                DisplayName = "TestFixture#1",
                Order = 1,
                Description = "Test fixture description",
                TestFixtureEntity = fixtureEntity,
                ScriptFile = "Script.csx",
                IsMuted = true
            };
            testFixtureViewModel.Tags.Add("color", "red");
            testFixtureViewModel.Tags.Add("priority", "high");

            Assert.That(testFixture.DisplayName, Is.EqualTo("TestFixture#1"));
            Assert.That(testFixture.Order, Is.EqualTo(1));
            Assert.That(testFixture.Description, Is.EqualTo("Test fixture description"));
            Assert.That(testFixture.TestFixtureEntity, Is.EqualTo(fixtureEntity));
            Assert.That(testFixtureViewModel.Tags.Contains("color"));
            Assert.That(testFixtureViewModel.Tags.Contains("priority"));
            Assert.That(testFixture.ScriptFile, Is.EqualTo("Script.csx"));
            Assert.That(testFixture.IsMuted);
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

            Assert.That(testCaseViewModel.IsVisible, Is.EqualTo(shouldBeVisible));
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
            testFixture.TestCases.Add(testCase.TestCaseId);
            TestFixtureViewModel testfixtureViewModel = new TestFixtureViewModel(testFixture);
            testfixtureViewModel.Tags.Add("module", "test explorer");
            testfixtureViewModel.AddTestCase(testCaseViewModel, Substitute.For<IProjectFileSystem>());
            testfixtureViewModel.UpdateVisibility(filterText);
            testCaseViewModel.UpdateVisibility(filterText);

            Assert.That(testfixtureViewModel.IsVisible, Is.EqualTo(shouldBeVisible));
        }
    }
}
