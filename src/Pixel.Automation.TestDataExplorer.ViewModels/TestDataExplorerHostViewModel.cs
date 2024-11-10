using Caliburn.Micro;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.TestDataExplorer.ViewModels
{
    /// <summary>
    /// Host control for the TestDataExplorer screen
    /// </summary>
    public class TestDataExplorerHostViewModel : AnchorableHost, ITestDataRepositoryHost
    {      
        /// <summary>
        /// Preferred location of the panel
        /// </summary>
        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Bottom; }
        }    

        private IScreen defaultContent = new MockTestDataExplorerViewModel();

        public TestDataExplorerHostViewModel()
        {
            this.DisplayName = "Test Data Explorer";
            _ = this.ActivateItemAsync(defaultContent);
        }

        /// <inheritdoc/>       
        public override async Task DeactivateItemAsync(IScreen item, bool close, CancellationToken cancellationToken = default)
        {
            await base.DeactivateItemAsync(item, close, cancellationToken);
            //we want the default screen to be activated whenever we deactivate any other screen
            if (!close && (item != defaultContent))
            {
                await this.ActivateItemAsync(defaultContent);
            }
        }
    }
}
