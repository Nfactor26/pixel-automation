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

            Assert.AreEqual(0, tagCollectionViewModel.Tags.Count);
            Assert.IsNull(tagCollectionViewModel.SelectedTag);
            Assert.IsFalse(tagCollectionViewModel.HasErrors);
            Assert.IsFalse(tagCollectionViewModel.CanEdit);
            Assert.IsFalse(tagCollectionViewModel.CanDelete);
        }

        /// <summary>
        /// Validate that it is possible to add a new Tag
        /// </summary>
        [TestCase]
        public void ValidateThatNewTagCanBeAdded()
        {
            var tagCollectionViewModel = new TagCollectionViewModel();
            tagCollectionViewModel.AddNew();

            Assert.AreEqual(1, tagCollectionViewModel.Tags.Count);
            Assert.IsNotNull(tagCollectionViewModel.SelectedTag);
            //when a new tag is added, it is automatically put in edit mode.
            //Hence, TagCollectionViewModel will CanEdit = false
            Assert.IsFalse(tagCollectionViewModel.CanEdit);
            Assert.IsTrue(tagCollectionViewModel.CanDelete);

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

            Assert.IsFalse(tagViewModel.IsEditing);

            tagCollectionViewModel.EditSelected();

            Assert.IsTrue(tagViewModel.IsEditing);          
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


            Assert.AreEqual(1, tagCollectionViewModel.Tags.Count);
            Assert.IsFalse(tagViewModel.IsDeleted);

            tagCollectionViewModel.DeleteSelected();

            Assert.AreEqual(0, tagCollectionViewModel.Tags.Count);
            Assert.IsTrue(tagViewModel.IsDeleted);
            Assert.IsNull(tagCollectionViewModel.SelectedTag);
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
            Assert.IsFalse(isValid);

            Assert.IsTrue(tagCollectionViewModel.HasErrors);
            Assert.AreEqual(3, validationErrors.Count());
            Assert.IsTrue(validationErrors.Contains("All tags must have a key and value."));
            Assert.IsTrue(validationErrors.Contains("All tags open for edit must be saved."));
            Assert.IsTrue(validationErrors.Contains("Tag keys must be unique"));

        }
    }
}
