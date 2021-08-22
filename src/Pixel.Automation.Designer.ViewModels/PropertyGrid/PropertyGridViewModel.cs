using Pixel.Automation.Editor.Core;
using System;
using System.Windows.Input;

namespace Pixel.Automation.Designer.ViewModels
{
    public class PropertyGridViewModel : Anchorable
    {
        public override double PreferredWidth => 320;

        private object selectedObject;
        public object SelectedObject
        {
            get { return selectedObject; }           
        }

        private bool isReadOnly;
        public bool IsReadOnly
        {
            get => isReadOnly;           
        }

        public bool ShowSaveButton => this.onSave != null;

        private Action onSave;

        private Func<bool> canSave;

        private ICommand saveCommand;
        public ICommand SaveCommand
        {
            get { return saveCommand ?? new RelayCommand(p => Save(), p => CanSave()); }
        }      

        public PropertyGridViewModel()
        {
            this.DisplayName = "Properties";
        }

        public void SetState(object selectedObject, bool isReadOnly, Action saveCommand, Func<bool> canSave)
        {
            this.selectedObject = selectedObject;
            this.isReadOnly = isReadOnly;
            this.onSave = saveCommand;
            this.canSave = canSave;
            NotifyOfPropertyChange(() => SelectedObject);
            NotifyOfPropertyChange(() => IsReadOnly);
            NotifyOfPropertyChange(() => ShowSaveButton);
            NotifyOfPropertyChange(() => SaveCommand);
        }

        private void Save()
        {
            if(this.onSave != null)
            {
                this.onSave();
            }           
        }

        private bool CanSave()
        {
            if(this.canSave != null)
            {
                return this.canSave();
            }
            return true;
        }
    }
}
