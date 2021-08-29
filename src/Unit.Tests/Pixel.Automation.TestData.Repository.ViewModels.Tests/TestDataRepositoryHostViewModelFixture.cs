using Caliburn.Micro;
using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Editor.Core;
using System.Threading.Tasks;

namespace Pixel.Automation.TestData.Repository.ViewModels.Tests
{
    /// <summary>
    /// Test Fixture for <see cref="TestDataRepositoryHostViewModel"/>
    /// </summary>
    [TestFixture]
    public class TestDataRepositoryHostViewModelFixture
    {
        /// <summary>
        /// Validate  that TestExplorerViewModel has a correct initial state after initialization
        /// </summary>
        [TestCase]
        public void ValidateThatTestDataRepositoryHostViewModelIsCorrectlyInitialized()
        {
            var testDataRepositoryHostViewModel = new TestDataRepositoryHostViewModel();

            Assert.AreEqual("Test Data Repository", testDataRepositoryHostViewModel.DisplayName);
            Assert.AreEqual(PaneLocation.Bottom, testDataRepositoryHostViewModel.PreferredLocation);
            Assert.IsNotNull(testDataRepositoryHostViewModel.ActiveItem);
        }

        /// <summary>
        /// when deactivating a screen, default screen should be set as the active item
        /// </summary>
        /// <returns></returns>
        [TestCase]
        public async Task ValidateThatDeactivatingActiveItemShouldSetDefaultContentAsActiveItem()
        {
            var testDataRepositoryHostViewModel = new TestDataRepositoryHostViewModel();
            var defaultScreen = testDataRepositoryHostViewModel.ActiveItem;
            var newScreen = Substitute.For<IScreen>();
            await testDataRepositoryHostViewModel.ActivateItemAsync(newScreen);

            Assert.AreEqual(newScreen, testDataRepositoryHostViewModel.ActiveItem);

            await testDataRepositoryHostViewModel.DeactivateItemAsync(newScreen, false);

            Assert.AreNotEqual(newScreen, testDataRepositoryHostViewModel.ActiveItem);
            Assert.AreEqual(defaultScreen, testDataRepositoryHostViewModel.ActiveItem);
        }
    }
}