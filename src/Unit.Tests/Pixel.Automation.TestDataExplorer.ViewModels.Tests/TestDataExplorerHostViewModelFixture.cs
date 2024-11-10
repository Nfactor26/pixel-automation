using Caliburn.Micro;
using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Editor.Core;
using System.Threading.Tasks;

namespace Pixel.Automation.TestDataExplorer.ViewModels.Tests
{
    /// <summary>
    /// Test Fixture for <see cref="TestDataRepositoryHostViewModel"/>
    /// </summary>
    [TestFixture]
    public class TestDataExplorerHostViewModelFixture
    {
        /// <summary>
        /// Validate  that TestExplorerViewModel has a correct initial state after initialization
        /// </summary>
        [TestCase]
        public void ValidateThatTestDataRepositoryHostViewModelIsCorrectlyInitialized()
        {
            var testDataRepositoryHostViewModel = new TestDataExplorerHostViewModel();

            Assert.That(testDataRepositoryHostViewModel.DisplayName, Is.EqualTo("Test Data Explorer"));
            Assert.That(testDataRepositoryHostViewModel.PreferredLocation, Is.EqualTo(PaneLocation.Bottom));
            Assert.That(testDataRepositoryHostViewModel.ActiveItem is not null);
        }

        /// <summary>
        /// when deactivating a screen, default screen should be set as the active item
        /// </summary>
        /// <returns></returns>
        [TestCase]
        public async Task ValidateThatDeactivatingActiveItemShouldSetDefaultContentAsActiveItem()
        {
            var testDataRepositoryHostViewModel = new TestDataExplorerHostViewModel();
            var defaultScreen = testDataRepositoryHostViewModel.ActiveItem;
            var newScreen = Substitute.For<IScreen>();
            await testDataRepositoryHostViewModel.ActivateItemAsync(newScreen);

            Assert.That(testDataRepositoryHostViewModel.ActiveItem, Is.EqualTo(newScreen));

            await testDataRepositoryHostViewModel.DeactivateItemAsync(newScreen, false);

            Assert.That(testDataRepositoryHostViewModel.ActiveItem, Is.Not.EqualTo(newScreen));
            Assert.That(testDataRepositoryHostViewModel.ActiveItem, Is.EqualTo(defaultScreen));
        }
    }
}