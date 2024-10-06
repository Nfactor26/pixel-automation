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

            Assert.That(editTestFixtureViewModel.TestFixtureDisplayName, Is.EqualTo(testFixture.DisplayName));
            Assert.That(editTestFixtureViewModel.TestFixtureCategory, Is.EqualTo(testFixture.Category));
            Assert.That(editTestFixtureViewModel.TestFixtureDescription, Is.EqualTo(testFixture.Description));
            Assert.That(editTestFixtureViewModel.IsMuted, Is.EqualTo(testFixture.IsMuted));
            Assert.That(editTestFixtureViewModel.DelayFactor, Is.EqualTo((100 - testFixture.DelayFactor)));
            Assert.That(editTestFixtureViewModel.Order, Is.EqualTo(testFixture.Order));         
            Assert.That(editTestFixtureViewModel.TagCollection.Tags.Count, Is.EqualTo(testFixture.Tags.Count));
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

            Assert.That(testFixture.DisplayName, Is.Not.EqualTo(editTestFixtureViewModel.TestFixtureDisplayName));
            Assert.That(testFixture.Category, Is.Not.EqualTo(editTestFixtureViewModel.TestFixtureCategory));
            Assert.That(testFixture.Description, Is.Not.EqualTo(editTestFixtureViewModel.TestFixtureDescription));           
            Assert.That(testFixture.IsMuted, Is.Not.EqualTo(editTestFixtureViewModel.IsMuted));
            Assert.That((100 - testFixture.DelayFactor), Is.Not.EqualTo(editTestFixtureViewModel.DelayFactor));
            Assert.That(testFixture.Order, Is.Not.EqualTo(editTestFixtureViewModel.Order));
            Assert.That(editTestFixtureViewModel.TagCollection.Tags.Count, Is.Not.EqualTo(testFixture.Tags.Count), "Tag count should not be equal");
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

            Assert.That(editTestFixtureViewModel.TestFixtureDisplayName, Is.EqualTo(testFixture.DisplayName));
            Assert.That(editTestFixtureViewModel.TestFixtureCategory, Is.EqualTo(testFixture.Category));
            Assert.That(editTestFixtureViewModel.TestFixtureDescription, Is.EqualTo(testFixture.Description));
            Assert.That(editTestFixtureViewModel.IsMuted, Is.EqualTo(testFixture.IsMuted));
            Assert.That(editTestFixtureViewModel.Order, Is.EqualTo(testFixture.Order));
            Assert.That(editTestFixtureViewModel.TagCollection.Tags.Count, Is.EqualTo(testFixture.Tags.Count), "Tag count should be equal");
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

            Assert.That(editTestFixtureViewModel.ShowModelErrors == false); // We don't show model error for validation with display name
            Assert.That(editTestFixtureViewModel.HasErrors);
            Assert.That(testFixture.DisplayName, Is.Not.EqualTo(editTestFixtureViewModel.TestFixtureDisplayName));

            //correct the issue with display name
            editTestFixtureViewModel.TestFixtureDisplayName = "TestFixture#3";
            Assert.That(editTestFixtureViewModel.ShowModelErrors == false);
            Assert.That(editTestFixtureViewModel.HasErrors == false);

            //set category to empty which is required 
            editTestFixtureViewModel.TestFixtureCategory = string.Empty;
            Assert.That(editTestFixtureViewModel.ShowModelErrors == false); // We don't show model error for validation with category name
            Assert.That(editTestFixtureViewModel.HasErrors);
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

            Assert.That(editTestFixtureViewModel.ShowModelErrors); // We don't show model error for validation with display name
            Assert.That(editTestFixtureViewModel.HasErrors);
            Assert.That(testFixture.Tags.Count, Is.Not.EqualTo(3));

            editTestFixtureViewModel.DismissModelErrors();
            Assert.That(editTestFixtureViewModel.ShowModelErrors == false);
            Assert.That(editTestFixtureViewModel.HasErrors == false);

            editTestFixtureViewModel.TagCollection.Tags.Remove(colorTag);
            var moduleTag = new TagViewModel(new Tag() { Key = "module", Value = "auth" });
            editTestFixtureViewModel.TagCollection.Add(moduleTag);
            await editTestFixtureViewModel.Save();

            Assert.That(editTestFixtureViewModel.ShowModelErrors == false);
            Assert.That(editTestFixtureViewModel.HasErrors == false);
            Assert.That(testFixture.Tags.Count, Is.EqualTo(3));
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
