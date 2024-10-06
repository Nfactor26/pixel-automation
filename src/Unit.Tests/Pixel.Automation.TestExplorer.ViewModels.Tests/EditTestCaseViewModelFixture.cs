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

          
            Assert.That(editTestCaseViewModel.TestCaseDisplayName, Is.EqualTo(testCase.DisplayName));
            Assert.That(editTestCaseViewModel.TestCaseDescrpition, Is.EqualTo(testCase.Description));
            Assert.That(editTestCaseViewModel.IsMuted, Is.EqualTo(testCase.IsMuted));
            Assert.That(editTestCaseViewModel.DelayFactor, Is.EqualTo((100 - testCase.DelayFactor)));
            Assert.That(editTestCaseViewModel.Order, Is.EqualTo(testCase.Order));
            Assert.That(editTestCaseViewModel.Priority, Is.EqualTo(testCase.Priority));
            Assert.That(editTestCaseViewModel.TagCollection.Tags.Count, Is.EqualTo(testCase.Tags.Count));            
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

            Assert.That(testCase.DisplayName, Is.Not.EqualTo(editTestCaseViewModel.TestCaseDisplayName));
            Assert.That(testCase.Description, Is.Not.EqualTo(editTestCaseViewModel.TestCaseDescrpition));
            Assert.That(testCase.Order, Is.Not.EqualTo(editTestCaseViewModel.Order));
            Assert.That(testCase.IsMuted, Is.Not.EqualTo(editTestCaseViewModel.IsMuted));
            Assert.That((100 - testCase.DelayFactor), Is.Not.EqualTo(editTestCaseViewModel.DelayFactor));         
            Assert.That(testCase.Priority, Is.Not.EqualTo(editTestCaseViewModel.Priority));
            Assert.That(editTestCaseViewModel.TagCollection.Tags.Count, Is.Not.EqualTo(testCase.Tags.Count), "Tag count should not be equal");
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

            Assert.That(testCase.DisplayName, Is.EqualTo(editTestCaseViewModel.TestCaseDisplayName));
            Assert.That(testCase.Description, Is.EqualTo(editTestCaseViewModel.TestCaseDescrpition));
            Assert.That(testCase.Order, Is.EqualTo(editTestCaseViewModel.Order));
            Assert.That(testCase.IsMuted, Is.EqualTo(editTestCaseViewModel.IsMuted));
            Assert.That((100 - testCase.DelayFactor), Is.EqualTo(editTestCaseViewModel.DelayFactor));
            Assert.That(testCase.Priority, Is.EqualTo(editTestCaseViewModel.Priority));
            Assert.That(editTestCaseViewModel.TagCollection.Tags.Count, Is.EqualTo(testCase.Tags.Count), "Tag count should be equal");
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

            Assert.That(editTestCaseViewModel.ShowModelErrors == false); // We don't show model error for validation with display name
            Assert.That(editTestCaseViewModel.HasErrors);
            Assert.That(testCase.DisplayName, Is.Not.EqualTo(editTestCaseViewModel.TestCaseDisplayName));
          
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
          
            Assert.That(editTestCaseViewModel.ShowModelErrors); // We don't show model error for validation with display name
            Assert.That(editTestCaseViewModel.HasErrors);
            Assert.That(testCase.Tags.Count, Is.Not.EqualTo(3));

            editTestCaseViewModel.DismissModelErrors();
            Assert.That(editTestCaseViewModel.ShowModelErrors == false);
            Assert.That(editTestCaseViewModel.HasErrors == false);

            editTestCaseViewModel.TagCollection.Tags.Remove(colorTag);
            var moduleTag = new TagViewModel(new Tag() { Key = "module", Value = "auth" });
            editTestCaseViewModel.TagCollection.Add(moduleTag);
            await editTestCaseViewModel.Save();
           
            Assert.That(editTestCaseViewModel.ShowModelErrors == false);
            Assert.That(editTestCaseViewModel.HasErrors == false);
            Assert.That(testCase.Tags.Count, Is.EqualTo(3));
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
