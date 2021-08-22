using Caliburn.Micro;
using System.Windows.Input;

namespace Pixel.Automation.Editor.Core
{
    public class HostToolBox : Conductor<IScreen>.Collection.OneActive, IToolBox
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
    }
}
