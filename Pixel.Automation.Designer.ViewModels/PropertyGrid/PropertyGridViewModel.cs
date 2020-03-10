using Pixel.Automation.Editor.Core;

namespace Pixel.Automation.Designer.ViewModels
{
    public class PropertyGridViewModel : ToolBox
    {
        public override double PreferredWidth => 320;

        private object selectedObject;
        public object SelectedObject
        {
            get { return selectedObject; }
            set
            {
                selectedObject = value;
                NotifyOfPropertyChange(() => SelectedObject);
            }
        }

        private bool isReadOnly;
        public bool IsReadOnly
        {
            get => isReadOnly;
            set
            {
                isReadOnly = value;
                NotifyOfPropertyChange(() => IsReadOnly);
            }
        }


        public PropertyGridViewModel()
        {
            this.DisplayName = "Properties";
        }
    }
}
