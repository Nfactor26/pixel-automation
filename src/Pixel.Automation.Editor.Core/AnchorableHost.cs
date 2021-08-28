using Caliburn.Micro;
using System.Windows.Input;

namespace Pixel.Automation.Editor.Core
{
    /// <summary>
    /// AnchorabeHost screen is an anchored conductor screen  i.e. it hosts other screens.
    /// </summary>
    public class AnchorableHost : Conductor<IScreen>.Collection.OneActive, IAnchorable
    {
        /// <inheritdoc/>
        public override bool IsActive => true;

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
        public virtual PaneLocation PreferredLocation
        {
            get { return PaneLocation.Left; }
        }

        /// <summary>
        /// Preferred width of the panel
        /// </summary>
        public virtual double PreferredWidth
        {
            get => 250;
        }

        /// <summary>
        /// Preferred height of the panel
        /// </summary>
        public virtual double PreferredHeight
        {
            get => 250;
        }     
     
        /// <summary>
        /// Guard method to indicate whether this panel can be closed
        /// </summary>
        /// <returns></returns>
        public virtual bool CanClose()
        {
            return true;
        }

        /// <summary>
        /// Hide the panel on clicking close button
        /// </summary>
        public virtual void CloseScreen()
        {
            this.IsVisible = false;
        }     
    }
}
