using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Automation.TestExplorer.ViewModels.Tests
{
    /// <summary>
    /// Test Fixture for <see cref="TagCollectionViewModel"/>
    /// </summary>
    [TestFixture]
    public class TagCollectionViewModelFixture
    {
        /// <summary>
        /// Validate TagCollectionViewModel has correct initial state after initialization
        /// </summary>
        [TestCase]
        public void ValidateThatTagCollectionViewModelCanBeInitialized()
        {
            var tagCollectionViewModel = new TagCollectionViewModel();

            Assert.That(tagCollectionViewModel.Tags.Count, Is.EqualTo(0));
            Assert.That(tagCollectionViewModel.SelectedTag is null);
            Assert.That(tagCollectionViewModel.HasErrors == false);
            Assert.That(tagCollectionViewModel.CanEdit == false);
            Assert.That(tagCollectionViewModel.CanDelete == false);
        }

        /// <summary>
        /// Validate that it is possible to add a new Tag
        /// </summary>
        [TestCase]
        public void ValidateThatNewTagCanBeAdded()
        {
            var tagCollectionViewModel = new TagCollectionViewModel();
            tagCollectionViewModel.AddNew();

            Assert.That(tagCollectionViewModel.Tags.Count, Is.EqualTo(1));
            Assert.That(tagCollectionViewModel.SelectedTag is not null);
            //when a new tag is added, it is automatically put in edit mode.
            //Hence, TagCollectionViewModel will CanEdit = false
            Assert.That(tagCollectionViewModel.CanEdit == false);
            Assert.That(tagCollectionViewModel.CanDelete);

        }

        /// <summary>
        /// Validate that it is possible to edit SelectedTag
        /// </summary>
        [TestCase]
        public void ValidateThatSelectedTagCanBeEdited()
        {
            var tagViewModel = new TagViewModel(new Core.TestData.Tag() { Key = "key", Value = "value" });
            var tagCollectionViewModel = new TagCollectionViewModel();
            tagCollectionViewModel.Add(tagViewModel);
            tagCollectionViewModel.SelectedTag = tagViewModel;

            Assert.That(tagViewModel.IsEditing == false);

            tagCollectionViewModel.EditSelected();

            Assert.That(tagViewModel.IsEditing);          
        }

        /// <summary>
        /// Validate that it is possible to delete SelectedTag
        /// </summary>
        [TestCase]
        public void ValidateThatSelectedTagCanBeDeleted ()
        {
            var tagViewModel = new TagViewModel(new Core.TestData.Tag() { Key = "key", Value = "value" });
            var tagCollectionViewModel = new TagCollectionViewModel();
            tagCollectionViewModel.Add(tagViewModel);
            tagCollectionViewModel.SelectedTag = tagViewModel;


            Assert.That(tagCollectionViewModel.Tags.Count, Is.EqualTo(1));
            Assert.That(tagViewModel.IsDeleted == false);

            tagCollectionViewModel.DeleteSelected();

            Assert.That(tagCollectionViewModel.Tags.Count, Is.EqualTo(0));
            Assert.That(tagViewModel.IsDeleted);
            Assert.That(tagCollectionViewModel.SelectedTag is null);
        }

        /// <summary>
        /// Validate that below validations are correctly done.
        /// 1.All tags must have non-empty key and value.
        /// 2.Duplicates keys are not allowed.
        /// 3.No Tag should be in Edit mode i.e. IsEditing != true.
        /// </summary>
        [Test]
        public void ValidateThatValidationWorksAsExpected()
        { 
            var tagCollectionViewModel = new TagCollectionViewModel();
            tagCollectionViewModel.Add(new TagViewModel(new Core.TestData.Tag() { Key = "tag1", Value = "value" }) { IsEditing = true});
            tagCollectionViewModel.Add(new TagViewModel(new Core.TestData.Tag() { Key = "tag2", Value = "value" }));
            tagCollectionViewModel.Add(new TagViewModel(new Core.TestData.Tag() { Key = "tag1", Value = "" }));

            var isValid = tagCollectionViewModel.Validate(out List<string> validationErrors);
            Assert.That(isValid == false);

            Assert.That(tagCollectionViewModel.HasErrors);
            Assert.That(validationErrors.Count(), Is.EqualTo(3));
            Assert.That(validationErrors.Contains("All tags must have a key and value."));
            Assert.That(validationErrors.Contains("All tags open for edit must be saved."));
            Assert.That(validationErrors.Contains("Tag keys must be unique"));

        }
    }
}
