using Caliburn.Micro;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    /// <summary>
    /// Host panel for test explorer for automation projects
    /// </summary>
    public class TestExplorerHostViewModel : Conductor<IScreen>.Collection.OneActive, ITestExplorerHost
    {
        public override bool IsActive => true;

        bool isActiveItem;
        /// <summary>
        /// Indicates if panel is the active item amongst multiple other docked panels
        /// </summary>
        public virtual bool IsActiveItem
        {
            get => isActiveItem;
            set
            {
                isActiveItem = value;
                NotifyOfPropertyChange(() => IsActiveItem);
            }

        }

        private bool isVisible = true;
        /// <summary>
        /// Indicates whether the panel is visible
        /// </summary>
        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                isVisible = value;
                NotifyOfPropertyChange(() => IsVisible);
            }
        }

        private bool isSelected;
        /// <summary>
        /// Indicates whether the panel is selected
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                NotifyOfPropertyChange(() => IsSelected);
            }
        }

        private ICommand closeCommand;
        /// <summary>
        /// Close command for the panel.
        /// </summary>
        public ICommand CloseCommand
        {
            get { return closeCommand ?? (closeCommand = new RelayCommand(p => CloseScreen(), p => CanClose())); }
        }

        /// <summary>
        /// Preferred location of the panel
        /// </summary>
        public PaneLocation PreferredLocation
        {
            get { return PaneLocation.Left; }
        }

        /// <summary>
        /// Preferred width of the panel
        /// </summary>
        public double PreferredWidth
        {
            get => 250;            
        }

        /// <summary>
        /// Preferred height of the panel
        /// </summary>
        public double PreferredHeight
        {
            get => 250;           
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

        /// <summary>
        /// Guard method to indicate whether this panel can be closed
        /// </summary>
        /// <returns></returns>
        public bool CanClose()
        {
            return true;
        }

        /// <summary>
        /// Hide the panel on clicking close button
        /// </summary>
        public void CloseScreen()
        {
            this.IsVisible = false;
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
