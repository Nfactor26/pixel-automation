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

            Assert.That(testExplorerHostViewModel.DisplayName, Is.EqualTo("Test Explorer"));
            Assert.That(testExplorerHostViewModel.PreferredLocation, Is.EqualTo(PaneLocation.Left));
            Assert.That(testExplorerHostViewModel.ActiveItem is not null);
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

            Assert.That(testExplorerHostViewModel.ActiveItem, Is.EqualTo(newScreen));

            await testExplorerHostViewModel.DeactivateItemAsync(newScreen, false);

            Assert.That(testExplorerHostViewModel.ActiveItem, Is.Not.EqualTo(newScreen));
            Assert.That(testExplorerHostViewModel.ActiveItem, Is.EqualTo(defaultScreen));
        }
    }
}
