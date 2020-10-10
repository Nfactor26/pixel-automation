using Caliburn.Micro;
using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Editor.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestCase = Pixel.Automation.Core.TestData.TestCase;

namespace Pixel.Automation.TestExplorer.ViewModels.Tests
{
    public class TestCaseViewModelFixture
    {
        private IEventAggregator eventAggregator;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            eventAggregator = Substitute.For<IEventAggregator>();
            eventAggregator.PublishOnBackgroundThreadAsync(Arg.Any<TestCaseUpdatedEventArgs>()).Returns(Task.CompletedTask);
        }

        [SetUp]
        public void Setup()
        {
            eventAggregator.ClearReceivedCalls();
        }

        /// <summary>
        /// Initialize a TestCaseViewModel from a TestCase object and verify that TestCaseViewModel properties returns correct values
        /// </summary>
        [Test]
        public void ValidateThatTestCaseViewModelReturnsCorrectValues()
        {
            var testCaseEntity = new Entity();
            var tags = new List<string>();
            tags.Add("Tag1");
            tags.Add("Tag2");
            TestCase testCase = new TestCase()
            {
                DisplayName = "TestCase#1",
                Order = 1,
                Description = "Test case description",
                TestCaseEntity = testCaseEntity,
                FixtureId = "Fixture#1",
                ScriptFile = "Script.csx",            
                Tags = tags
            };
            TestCaseViewModel testCaseViewModel = new TestCaseViewModel(testCase, eventAggregator);

            Assert.IsTrue(!string.IsNullOrEmpty(testCaseViewModel.Id));
            Assert.AreEqual("TestCase#1", testCaseViewModel.DisplayName);
            Assert.AreEqual(1, testCaseViewModel.Order);
            Assert.AreEqual("Test case description", testCaseViewModel.Description);
            Assert.AreEqual("Fixture#1", testCaseViewModel.FixtureId);
            Assert.AreEqual(testCaseEntity, testCaseViewModel.TestCaseEntity);
            Assert.AreEqual(tags, testCaseViewModel.Tags);
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
            var tags = new List<string>();
            tags.Add("Tag1");
            tags.Add("Tag2");
            TestCase testCase = new TestCase();
            _ = new TestCaseViewModel(testCase, eventAggregator)
            {
                DisplayName = "TestCase#1",
                Order = 1,
                Description = "Test case description",
                TestCaseEntity = testCaseEntity,               
                ScriptFile = "Script.csx",
                IsMuted = true,
                Tags = tags
            };

            Assert.AreEqual("TestCase#1", testCase.DisplayName);
            Assert.AreEqual(1, testCase.Order);
            Assert.AreEqual("Test case description", testCase.Description);         
            Assert.AreEqual(testCaseEntity, testCase.TestCaseEntity);
            Assert.AreEqual(tags, testCase.Tags);
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
            TestCaseViewModel testCaseViewModel = new TestCaseViewModel(testCase, eventAggregator);
            testCaseViewModel.SetTestDataSource(dataSourceId);

            Assert.AreEqual(shouldBePossibleToEdit, testCaseViewModel.CanOpenForEdit);
            eventAggregator.Received(1).PublishOnBackgroundThreadAsync(Arg.Any<TestCaseUpdatedEventArgs>());
        }

        /// <summary>
        /// Validate that visibility property is properly updated based on filter text applied
        /// </summary>
        /// <param name="filterText"></param>
        /// <param name="shouldBeVisible"></param>
        [TestCase("#1", true)]
        [TestCase("Test", true)]
        [TestCase("Case", true)]
        [TestCase("", true)]
        [TestCase("#2", false)]
        public void ValidateThatVisibilityIsCorrectlyUpdatedWhenFilterTextChanges(string filterText, bool shouldBeVisible)
        {
            TestCase testCase = new TestCase()
            {
                DisplayName = "TestCase#1"
            };
            TestCaseViewModel testCaseViewModel = new TestCaseViewModel(testCase, eventAggregator);
            testCaseViewModel.UpdateVisibility(filterText);

            Assert.AreEqual(shouldBeVisible, testCaseViewModel.IsVisible);
            
        }
    }
}