using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.TestData;
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

            Assert.IsNotNull(testCaseViewModel.TestCase);
            Assert.IsTrue(!string.IsNullOrEmpty(testCaseViewModel.TestCaseId));
            Assert.AreEqual("TestCase#1", testCaseViewModel.DisplayName);
            Assert.AreEqual(1, testCaseViewModel.Order);
            Assert.AreEqual("Test case description", testCaseViewModel.Description);
            Assert.AreEqual("Fixture#1", testCaseViewModel.FixtureId);
            Assert.AreEqual(testCaseEntity, testCaseViewModel.TestCaseEntity);
            Assert.IsTrue(testCaseViewModel.Tags.Contains("color"));
            Assert.IsTrue(testCaseViewModel.Tags.Contains("priority"));
            Assert.AreEqual("Script.csx", testCaseViewModel.ScriptFile);
            Assert.IsNull(testCaseViewModel.TestDataId);
            Assert.IsFalse(testCaseViewModel.IsMuted);
            Assert.IsFalse(testCaseViewModel.IsSelected);
            Assert.IsTrue(testCaseViewModel.IsVisible);
            Assert.IsFalse(testCaseViewModel.IsOpenForEdit);
            Assert.IsFalse(testCaseViewModel.CanOpenForEdit);
            Assert.IsFalse(testCaseViewModel.IsRunning);
            Assert.NotNull(testCaseViewModel.TestResults);  

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
        
            Assert.AreEqual("TestCase#1", testCase.DisplayName);
            Assert.AreEqual(1, testCase.Order);
            Assert.AreEqual("Test case description", testCase.Description);         
            Assert.AreEqual(testCaseEntity, testCase.TestCaseEntity);
            Assert.AreEqual(testCase.Tags["color"], "red");
            Assert.AreEqual("Script.csx", testCase.ScriptFile);
            Assert.IsTrue(testCase.IsMuted);
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

            Assert.AreEqual(shouldBePossibleToEdit, testCaseViewModel.CanOpenForEdit);           
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

            Assert.AreEqual(shouldBeVisible, testCaseViewModel.IsVisible);
            
        }
    }
}