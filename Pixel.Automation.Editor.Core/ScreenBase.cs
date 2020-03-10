using Caliburn.Micro;
using System.Windows.Input;

namespace Pixel.Automation.Editor.Core
{
    public abstract class ScreenBase : Screen
    {
        private bool isVisible = true;
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
        public ICommand CloseCommand
        {
            get { return closeCommand ?? (closeCommand = new RelayCommand(p => CloseScreen(), p => CanClose())); }
        }

        public abstract void CloseScreen();

        public abstract bool CanClose();
       
    }
}
