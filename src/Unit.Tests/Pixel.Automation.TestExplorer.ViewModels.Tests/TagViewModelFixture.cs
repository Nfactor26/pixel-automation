using NUnit.Framework;

namespace Pixel.Automation.TestExplorer.ViewModels.Tests
{
    /// <summary>
    /// Test Fixture for <see cref="TagViewModel"/>
    /// </summary>
    [TestFixture]
    public class TagViewModelFixture
    {
        /// <summary>
        /// Validate that initial values are as expected when TagViewModel is initialized
        /// </summary>
        [TestCase]
        public void ValidateThatTagViewModelCanBeInitialized()
        {
            var tagViewModel = new TagViewModel(new Core.TestData.Tag() { Key = "tag", Value = "value" });

            Assert.AreEqual("tag", tagViewModel.Key);
            Assert.AreEqual("value", tagViewModel.Value);
            Assert.IsFalse(tagViewModel.IsEditing);
            Assert.IsFalse(tagViewModel.IsDeleted);
            Assert.IsNotNull(tagViewModel.SaveCommand);
            Assert.AreEqual("tag : value", tagViewModel.ToString());

        }

        /// <summary>
        /// On Edit, IsEditing should be updated to true
        /// </summary>
        [TestCase]
        public void ValidateEditOperationOnTagViewModel()
        {
            var tagViewModel = new TagViewModel(new Core.TestData.Tag() { Key = "tag", Value = "value" });
            tagViewModel.Edit();

            Assert.IsTrue(tagViewModel.IsEditing);
        }

        /// <summary>
        /// On Delete, IsEditing should be updated to false and IsDelete should be updated to true
        /// </summary>
        [TestCase]
        public void ValidateDeleteOperationOnTagViewModel()
        {
            var tagViewModel = new TagViewModel(new Core.TestData.Tag() { Key = "tag", Value = "value" });

            tagViewModel.Delete();

            Assert.IsFalse(tagViewModel.IsEditing);
            Assert.IsTrue(tagViewModel.IsDeleted);
        }

        /// <summary>
        /// On Save, IsEdited should be set to false when tag data is valid
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expected"></param>
        [TestCase("tag", "value", false)]
        [TestCase("tag", "", true)]
        [TestCase("", "value", true)]
        public void ValidateSaveOperationOnTagViewModel(string key, string value, bool expected)
        {
            var tagViewModel = new TagViewModel(new Core.TestData.Tag() { Key = key, Value = value });

            tagViewModel.Edit();
            tagViewModel.Save();

            Assert.AreEqual(expected, tagViewModel.IsEditing);
        }

    }
}
