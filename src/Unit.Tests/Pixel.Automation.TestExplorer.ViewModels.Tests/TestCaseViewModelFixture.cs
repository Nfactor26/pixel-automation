using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.TestData;
using System.Diagnostics.CodeAnalysis;
using TestCase = Pixel.Automation.Core.TestData.TestCase;

namespace Pixel.Automation.TestExplorer.ViewModels.Tests
{
    /// <summary>
    /// Test Fixture for <see cref="TestCaseViewModel"/>
    /// </summary>
    [TestFixture]
    public class TestCaseViewModelFixture
    { 
        /// <summary>
        /// Initialize a TestCaseViewModel from a TestCase object and verify the initial state 
        /// </summary>
        [Test]
        public void ValidateThatTestCaseViewModelIsCorrectlyInitialized()
        {
            var testCaseEntity = new Entity();        
            TestCase testCase = new TestCase()
            {
                DisplayName = "TestCase#1",
                Order = 1,
                Description = "Test case description",
                TestCaseEntity = testCaseEntity,
                FixtureId = "Fixture#1",
                ScriptFile = "Script.csx"
            };
            testCase.Tags.Add("color", "red");
            testCase.Tags.Add("priority", "high");
            TestCaseViewModel testCaseViewModel = new TestCaseViewModel(testCase);

            Assert.That(testCaseViewModel.TestCase is not null);
            Assert.That(!string.IsNullOrEmpty(testCaseViewModel.TestCaseId));
            Assert.That(testCaseViewModel.DisplayName, Is.EqualTo("TestCase#1"));
            Assert.That(testCaseViewModel.Order, Is.EqualTo(1));
            Assert.That(testCaseViewModel.Description, Is.EqualTo("Test case description"));
            Assert.That(testCaseViewModel.FixtureId, Is.EqualTo("Fixture#1"));
            Assert.That(testCaseViewModel.TestCaseEntity, Is.EqualTo(testCaseEntity));
            Assert.That(testCaseViewModel.Tags.Contains("color"));
            Assert.That(testCaseViewModel.Tags.Contains("priority"));
            Assert.That(testCaseViewModel.ScriptFile, Is.EqualTo("Script.csx"));
            Assert.That(testCaseViewModel.TestDataId is null);
            Assert.That(testCaseViewModel.IsSelected == false);
            Assert.That(testCaseViewModel.IsVisible);
            Assert.That(testCaseViewModel.IsOpenForEdit == false);
            Assert.That(testCaseViewModel.CanOpenForEdit == false);
            Assert.That(testCaseViewModel.IsRunning == false);
            Assert.That(testCaseViewModel.TestResults is not null);  

        }

        /// <summary>
        /// Initialize a TestCaseViewModel from a TestCase object and verify that TestCaseViewModel properties returns correct values
        /// </summary>
        [Test]
        public void ValidateThatTestCaseValuesCanBeUpdatedByUpdatingWrapperPropertiesOnViewModel()
        {
            var testCaseEntity = new Entity();          
            TestCase testCase = new TestCase();
            var testCaseViewModel = new TestCaseViewModel(testCase)
            {
                DisplayName = "TestCase#1",
                Order = 1,
                Description = "Test case description",
                TestCaseEntity = testCaseEntity,               
                ScriptFile = "Script.csx",
                IsMuted = true
            };
            testCaseViewModel.Tags.Add("color", "red");       
        
            Assert.That(testCase.DisplayName, Is.EqualTo("TestCase#1"));
            Assert.That(testCase.Order, Is.EqualTo(1));
            Assert.That(testCase.Description, Is.EqualTo("Test case description"));         
            Assert.That(testCase.TestCaseEntity, Is.EqualTo(testCaseEntity));
            Assert.That(testCase.Tags["color"], Is.EqualTo("red"));
            Assert.That(testCase.ScriptFile, Is.EqualTo("Script.csx"));
            Assert.That(testCase.IsMuted);
        }

        /// <summary>
        /// Validate that CanEdit property is true only when TestDataId is set up for a test case
        /// </summary>
        /// <param name="dataSourceId"></param>
        /// <param name="shouldBePossibleToEdit"></param>
        [TestCase("DataSourceID", true)]
        [TestCase("", false)]
        public void ValidateThatCanEditPropertyIsTrueOnlyWhenDataSourceIDIsSetForTestCase(string dataSourceId, bool shouldBePossibleToEdit)
        {
            TestCase testCase = new TestCase()
            {
                TestDataId = dataSourceId
            };
            TestCaseViewModel testCaseViewModel = new TestCaseViewModel(testCase);
            testCaseViewModel.SetTestDataSource(dataSourceId);

            Assert.That(testCaseViewModel.CanOpenForEdit, Is.EqualTo(shouldBePossibleToEdit));           
        }

        /// <summary>
        /// Validate that visibility property is properly updated based on filter text applied
        /// </summary>
        /// <param name="filterText"></param>
        /// <param name="shouldBeVisible"></param>
        [TestCase("#1", true)]
        [TestCase("Test", true)]
        [TestCase("Case", true)]
        [TestCase("TestCase#1", true)]
        [TestCase("", true)]
        [TestCase("#2", false)]
        [TestCase("name:", true)] 
        [TestCase("name:#1:Test", true)] 
        [TestCase("name:#1", true)]        
        [TestCase("module:test", false)] 
        [TestCase("module:test explorer", true)]
        public void ValidateThatVisibilityIsCorrectlyUpdatedWhenFilterTextChanges(string filterText, bool shouldBeVisible)
        {
            TestCase testCase = new TestCase()
            {
                DisplayName = "TestCase#1"               
            };
            TestCaseViewModel testCaseViewModel = new TestCaseViewModel(testCase);           
            testCaseViewModel.Tags.Add("module", "test explorer");

            testCaseViewModel.UpdateVisibility(filterText);

            Assert.That(testCaseViewModel.IsVisible, Is.EqualTo(shouldBeVisible));
            
        }
    }
}