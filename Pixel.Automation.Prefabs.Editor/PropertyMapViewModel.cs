using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Prefabs.Editor
{
    public class PropertyMapViewModel : NotifyPropertyChanged
    {
        private PropertyMap propertyMap;
        public PropertyMap PropertyMap
        {
            get => propertyMap;
            set
            {
                propertyMap = value;
                OnPropertyChanged();
            }
        }

        public BindableCollection<string> PossibleMaps { get; set; } = new BindableCollection<string>();

    
    }
}
