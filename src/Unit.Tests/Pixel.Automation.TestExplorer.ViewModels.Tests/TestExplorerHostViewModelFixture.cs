using Caliburn.Micro;
using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Editor.Core;
using System.Threading.Tasks;

namespace Pixel.Automation.TestExplorer.ViewModels.Tests
{
    /// <summary>
    /// Test Fixture for <see cref="TestExplorerHostViewModel"/>
    /// </summary>
    [TestFixture]
    public class TestExplorerHostViewModelFixture
    {
        /// <summary>
        /// Validate  that TestExplorerViewModel has a correct initial state after initialization
        /// </summary>
        [TestCase]
        public void ValidateThatTextExplorerHostViewModelIsCorrectlyInitialized()
        {
            var testExplorerHostViewModel = new TestExplorerHostViewModel();

            Assert.AreEqual("Test Explorer", testExplorerHostViewModel.DisplayName);
            Assert.AreEqual(PaneLocation.Left, testExplorerHostViewModel.PreferredLocation);
            Assert.IsNotNull(testExplorerHostViewModel.ActiveItem);
        }

        /// <summary>
        /// when deactivating a screen, default screen should be set as the active item
        /// </summary>
        /// <returns></returns>
        [TestCase]
        public async Task ValidateThatDeactivatingActiveItemShouldSetDefaultContentAsActiveItem()
        {
            var testExplorerHostViewModel = new TestExplorerHostViewModel();
            var defaultScreen = testExplorerHostViewModel.ActiveItem;
            var newScreen = Substitute.For<IScreen>();
            await testExplorerHostViewModel.ActivateItemAsync(newScreen);

            Assert.AreEqual(newScreen, testExplorerHostViewModel.ActiveItem);

            await testExplorerHostViewModel.DeactivateItemAsync(newScreen, false);

            Assert.AreNotEqual(newScreen, testExplorerHostViewModel.ActiveItem);
            Assert.AreEqual(defaultScreen, testExplorerHostViewModel.ActiveItem);
        }
    }
}
