using Caliburn.Micro;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    /// <summary>
    /// Host panel for test explorer for automation projects
    /// </summary>
    public class TestExplorerHostViewModel : AnchorableHost, ITestExplorerHost
    {       
        /// <summary>
        /// Preferred location of the panel
        /// </summary>
        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Left; }
        }

       
        private IScreen defaultContent = new MockTestExplorerViewModel();

        /// <summary>
        /// constructor
        /// </summary>
        public TestExplorerHostViewModel()
        {
            this.DisplayName = "Test Explorer";
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
