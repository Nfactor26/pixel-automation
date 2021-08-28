using NUnit.Framework;
using Pixel.Automation.Core.TestData;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.TestExplorer.ViewModels.Tests
{
    /// <summary>
    /// Test Fixture for <see cref="EditTestCaseViewModel"/>
    /// </summary>
    [TestFixture]
    public class EditTestCaseViewModelFixture
    {
        /// <summary>
        /// Wrapped properties should reflect the underlying values of actual TestCase
        /// </summary>
        [Test]
        public void ValidateThatEditTestCaseViewModelIsCorrectlyInitialized()
        {
            var testCase = CreateTestCase();
            EditTestCaseViewModel editTestCaseViewModel = new EditTestCaseViewModel(testCase, Enumerable.Empty<string>());

          
            Assert.AreEqual(testCase.DisplayName, editTestCaseViewModel.TestCaseDisplayName);
            Assert.AreEqual(testCase.Description, editTestCaseViewModel.TestCaseDescrpition);
            Assert.AreEqual(testCase.IsMuted, editTestCaseViewModel.IsMuted);
            Assert.AreEqual((100 - testCase.DelayFactor), editTestCaseViewModel.DelayFactor);
            Assert.AreEqual(testCase.Order, editTestCaseViewModel.Order);
            Assert.AreEqual(testCase.Priority, editTestCaseViewModel.Priority);
            Assert.AreEqual(testCase.Tags.Count, editTestCaseViewModel.TagCollection.Tags.Count);            
        }

        /// <summary>
        /// Changes should not be applied if user cancels the edit
        /// </summary>
        [Test]
        public async Task ValidateThatChangesAreNotAppliedToTestCaseIfUserClicksCancelAfterMakingChanges()
        {
            var testCase = CreateTestCase();
            EditTestCaseViewModel editTestCaseViewModel = new EditTestCaseViewModel(testCase, Enumerable.Empty<string>());
            ApplyEdits(editTestCaseViewModel);

            await editTestCaseViewModel.Cancel();

            Assert.AreNotEqual(editTestCaseViewModel.TestCaseDisplayName, testCase.DisplayName);
            Assert.AreNotEqual(editTestCaseViewModel.TestCaseDescrpition, testCase.Description);
            Assert.AreNotEqual(editTestCaseViewModel.Order, testCase.Order);
            Assert.AreNotEqual(editTestCaseViewModel.IsMuted, testCase.IsMuted);
            Assert.AreNotEqual(editTestCaseViewModel.DelayFactor,  (100 - testCase.DelayFactor));         
            Assert.AreNotEqual(editTestCaseViewModel.Priority, testCase.Priority);
            Assert.AreNotEqual(editTestCaseViewModel.TagCollection.Tags.Count, testCase.Tags.Count, "Tag count should not be equal");
        }

        /// <summary>
        /// If there are no validation errors after edit, changes should be applied
        /// </summary>
        [Test]
        public async Task ValidateThatChangesAreAppliedToTestCaseIfUserClicksSaveAfterMakingChanges()
        {
            var testCase = CreateTestCase();
            EditTestCaseViewModel editTestCaseViewModel = new EditTestCaseViewModel(testCase, Enumerable.Empty<string>());
            ApplyEdits(editTestCaseViewModel);

            await editTestCaseViewModel.Save();

            Assert.AreEqual(editTestCaseViewModel.TestCaseDisplayName, testCase.DisplayName);
            Assert.AreEqual(editTestCaseViewModel.TestCaseDescrpition, testCase.Description);
            Assert.AreEqual(editTestCaseViewModel.Order, testCase.Order);
            Assert.AreEqual(editTestCaseViewModel.IsMuted, testCase.IsMuted);
            Assert.AreEqual(editTestCaseViewModel.DelayFactor, (100 - testCase.DelayFactor));
            Assert.AreEqual(editTestCaseViewModel.Priority, testCase.Priority);
            Assert.AreEqual(editTestCaseViewModel.TagCollection.Tags.Count, testCase.Tags.Count, "Tag count should be equal");
        }

        /// <summary>
        /// If there are validations errors after edit, changes should not be applied.      
        /// </summary>
        [Test]
        public async Task ValidateThatChangesAreNotAppliedToTestCaseWhenValidationErrorIfUserClicksSaveAfterMakingChanges()
        {
            var testCase = CreateTestCase();
            EditTestCaseViewModel editTestCaseViewModel = new EditTestCaseViewModel(testCase, new string[] { "TestCase#2" });
            ApplyEdits(editTestCaseViewModel);

            await editTestCaseViewModel.Save();

            Assert.IsFalse(editTestCaseViewModel.ShowModelErrors); // We don't show model error for validation with display name
            Assert.IsTrue(editTestCaseViewModel.HasErrors);
            Assert.AreNotEqual(editTestCaseViewModel.TestCaseDisplayName, testCase.DisplayName);
          
        }

        /// <summary>
        /// If there are validation errors with tags, a model error should be visible to user.
        /// User should be able to clear the model error, correct the issue and save which should apply the edit.
        /// </summary>
        [Test]
        public async Task ValidateThatModelErrosShouldBeVisibleWhenTagCollectionIsInvalid()
        {
            var testCase = CreateTestCase();
            EditTestCaseViewModel editTestCaseViewModel = new EditTestCaseViewModel(testCase, Enumerable.Empty<string>());
            var colorTag = new TagViewModel(new Tag() { Key = "color", Value = "yellow" });
            editTestCaseViewModel.TagCollection.Add(colorTag); //color is an existing key

            await editTestCaseViewModel.Save();
          
            Assert.IsTrue(editTestCaseViewModel.ShowModelErrors); // We don't show model error for validation with display name
            Assert.IsTrue(editTestCaseViewModel.HasErrors);
            Assert.AreNotEqual(3, testCase.Tags.Count);

            editTestCaseViewModel.DismissModelErrors();
            Assert.IsFalse(editTestCaseViewModel.ShowModelErrors);
            Assert.IsFalse(editTestCaseViewModel.HasErrors);

            editTestCaseViewModel.TagCollection.Tags.Remove(colorTag);
            var moduleTag = new TagViewModel(new Tag() { Key = "module", Value = "auth" });
            editTestCaseViewModel.TagCollection.Add(moduleTag);
            await editTestCaseViewModel.Save();
           
            Assert.IsFalse(editTestCaseViewModel.ShowModelErrors);
            Assert.IsFalse(editTestCaseViewModel.HasErrors);
            Assert.AreEqual(3, testCase.Tags.Count);
        }

        TestCase CreateTestCase()
        {
            TestCase testCase = new TestCase()
            {
                DisplayName = "TestCase#1",
                Order = 1,
                Description = "Test case description",
                TestCaseEntity = new Core.Entity(),
                FixtureId = "Fixture#1",
                ScriptFile = "Script.csx",
                DelayFactor = 10,
                Priority = Core.Enums.Priority.High
            };
            testCase.Tags.Add("color", "red");
            testCase.Tags.Add("priority", "high");
            return testCase;
        }

        void ApplyEdits(EditTestCaseViewModel editTestCaseViewModel)
        {
            editTestCaseViewModel.TestCaseDisplayName = "TestCase#2";
            editTestCaseViewModel.TestCaseDescrpition = string.Empty;
            editTestCaseViewModel.Order = 2;
            editTestCaseViewModel.IsMuted = true;
            editTestCaseViewModel.DelayFactor = 15;
            editTestCaseViewModel.Priority = Core.Enums.Priority.Low;
            editTestCaseViewModel.TagCollection.Add(new TagViewModel(new Tag() { Key = "module", Value = "auth" }));
        }
    }
}
