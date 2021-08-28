using NUnit.Framework;
using Pixel.Automation.Core.TestData;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.TestExplorer.ViewModels.Tests
{
    /// <summary>
    /// Test Fixture for <see cref="EditTestFixtureViewModel"/>
    /// </summary>
    [TestFixture]
    public class EditTestFixtureViewModelFixture
    {
        /// <summary>
        /// Wrapped properties should reflect the underlying values of actual TestCase
        /// </summary>
        [Test]
        public void ValidateThatEditTestFixtureViewModelCanBeInitialized()
        {
            var testFixture = CreateTestFixture();
            EditTestFixtureViewModel editTestFixtureViewModel = new EditTestFixtureViewModel(testFixture, Enumerable.Empty<string>());

            Assert.AreEqual(testFixture.DisplayName, editTestFixtureViewModel.TestFixtureDisplayName);
            Assert.AreEqual(testFixture.Category, editTestFixtureViewModel.TestFixtureCategory);
            Assert.AreEqual(testFixture.Description, editTestFixtureViewModel.TestFixtureDescription);
            Assert.AreEqual(testFixture.IsMuted, editTestFixtureViewModel.IsMuted);
            Assert.AreEqual((100 - testFixture.DelayFactor), editTestFixtureViewModel.DelayFactor);
            Assert.AreEqual(testFixture.Order, editTestFixtureViewModel.Order);         
            Assert.AreEqual(testFixture.Tags.Count, editTestFixtureViewModel.TagCollection.Tags.Count);
        }

        /// <summary>
        /// Changes should not be applied if user cancels the edit
        /// </summary>
        [Test]
        public async Task ValidateThatChangesAreNotAppliedToTestFixtureIfUserClicksCancelAfterMakingChanges()
        {
            var testFixture = CreateTestFixture();
            EditTestFixtureViewModel editTestFixtureViewModel = new EditTestFixtureViewModel(testFixture, Enumerable.Empty<string>());
            ApplyEdits(editTestFixtureViewModel);

            await editTestFixtureViewModel.Cancel();

            Assert.AreNotEqual(editTestFixtureViewModel.TestFixtureDisplayName, testFixture.DisplayName);
            Assert.AreNotEqual(editTestFixtureViewModel.TestFixtureCategory, testFixture.Category);
            Assert.AreNotEqual(editTestFixtureViewModel.TestFixtureDescription, testFixture.Description);           
            Assert.AreNotEqual(editTestFixtureViewModel.IsMuted, testFixture.IsMuted);
            Assert.AreNotEqual(editTestFixtureViewModel.DelayFactor, (100 - testFixture.DelayFactor));
            Assert.AreNotEqual(editTestFixtureViewModel.Order, testFixture.Order);
            Assert.AreNotEqual(editTestFixtureViewModel.TagCollection.Tags.Count, testFixture.Tags.Count, "Tag count should not be equal");
        }

        /// <summary>
        /// If there are no validation errors after edit, changes should be applied
        /// </summary>
        [Test]
        public async Task ValidateThatChangesAreAppliedToTestCaseIfUserClicksSaveAfterMakingChanges()
        {
            var testFixture = CreateTestFixture();
            EditTestFixtureViewModel editTestFixtureViewModel = new EditTestFixtureViewModel(testFixture, Enumerable.Empty<string>());
            ApplyEdits(editTestFixtureViewModel);


            await editTestFixtureViewModel.Save();

            Assert.AreEqual(editTestFixtureViewModel.TestFixtureDisplayName, testFixture.DisplayName);
            Assert.AreEqual(editTestFixtureViewModel.TestFixtureCategory, testFixture.Category);
            Assert.AreEqual(editTestFixtureViewModel.TestFixtureDescription, testFixture.Description);
            Assert.AreEqual(editTestFixtureViewModel.IsMuted, testFixture.IsMuted);
            Assert.AreEqual(editTestFixtureViewModel.DelayFactor, (100 - testFixture.DelayFactor));
            Assert.AreEqual(editTestFixtureViewModel.Order, testFixture.Order);
            Assert.AreEqual(editTestFixtureViewModel.TagCollection.Tags.Count, testFixture.Tags.Count, "Tag count should be equal");
        }

        /// <summary>
        /// If there are validations errors after edit, changes should not be applied.      
        /// </summary>
        [Test]
        public async Task ValidateThatChangesAreNotAppliedToTestFixtureWhenValidationErrorIfUserClicksSaveAfterMakingChanges()
        {
            var testFixture = CreateTestFixture();
            EditTestFixtureViewModel editTestFixtureViewModel = new EditTestFixtureViewModel(testFixture, new string[] { "TestFixture#2" });
            ApplyEdits(editTestFixtureViewModel);

            await editTestFixtureViewModel.Save();

            Assert.IsFalse(editTestFixtureViewModel.ShowModelErrors); // We don't show model error for validation with display name
            Assert.IsTrue(editTestFixtureViewModel.HasErrors);
            Assert.AreNotEqual(editTestFixtureViewModel.TestFixtureDisplayName, testFixture.DisplayName);

            //correct the issue with display name
            editTestFixtureViewModel.TestFixtureDisplayName = "TestFixture#3";
            Assert.IsFalse(editTestFixtureViewModel.ShowModelErrors);
            Assert.IsFalse(editTestFixtureViewModel.HasErrors);

            //set category to empty which is required 
            editTestFixtureViewModel.TestFixtureCategory = string.Empty;
            Assert.IsFalse(editTestFixtureViewModel.ShowModelErrors); // We don't show model error for validation with category name
            Assert.IsTrue(editTestFixtureViewModel.HasErrors);
        }

        /// <summary>
        /// If there are validation errors with tags, a model error should be visible to user.
        /// User should be able to clear the model error, correct the issue and save which should apply the edit.
        /// </summary>
        [Test]
        public async Task ValidateThatModelErrosShouldBeVisibleWhenTagCollectionIsInvalid()
        {
            var testFixture = CreateTestFixture();
            EditTestFixtureViewModel editTestFixtureViewModel = new EditTestFixtureViewModel(testFixture, new string[] { "TestFixture#2" });      
            var colorTag = new TagViewModel(new Tag() { Key = "color", Value = "yellow" });
            editTestFixtureViewModel.TagCollection.Add(colorTag); //color is an existing key

            await editTestFixtureViewModel.Save();

            Assert.IsTrue(editTestFixtureViewModel.ShowModelErrors); // We don't show model error for validation with display name
            Assert.IsTrue(editTestFixtureViewModel.HasErrors);
            Assert.AreNotEqual(3, testFixture.Tags.Count);

            editTestFixtureViewModel.DismissModelErrors();
            Assert.IsFalse(editTestFixtureViewModel.ShowModelErrors);
            Assert.IsFalse(editTestFixtureViewModel.HasErrors);

            editTestFixtureViewModel.TagCollection.Tags.Remove(colorTag);
            var moduleTag = new TagViewModel(new Tag() { Key = "module", Value = "auth" });
            editTestFixtureViewModel.TagCollection.Add(moduleTag);
            await editTestFixtureViewModel.Save();

            Assert.IsFalse(editTestFixtureViewModel.ShowModelErrors);
            Assert.IsFalse(editTestFixtureViewModel.HasErrors);
            Assert.AreEqual(3, testFixture.Tags.Count);
        }

        TestFixture CreateTestFixture()
        {
            TestFixture testFixture = new TestFixture()
            {
                DisplayName = "TestFixture#1",
                Category = "Category#1",
                Order = 1,
                Description = "Test fixture description",
                TestFixtureEntity = new Core.Entity(),             
                ScriptFile = "Script.csx",
                DelayFactor = 10
            };
            testFixture.Tags.Add("color", "red");
            testFixture.Tags.Add("priority", "high");
            return testFixture;
        }

        void ApplyEdits(EditTestFixtureViewModel editTestFixtureViewModel)
        {
            editTestFixtureViewModel.TestFixtureDisplayName = "TestFixture#2";
            editTestFixtureViewModel.TestFixtureCategory = "Category#2";
            editTestFixtureViewModel.TestFixtureDescription = string.Empty;
            editTestFixtureViewModel.Order = 2;
            editTestFixtureViewModel.IsMuted = true;
            editTestFixtureViewModel.DelayFactor = 15;          
            editTestFixtureViewModel.TagCollection.Add(new TagViewModel(new Tag() { Key = "module", Value = "auth" }));
        }
    }
}
