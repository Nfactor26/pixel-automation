using System;

namespace Pixel.Automation.Core.Models
{
    public class PropertyMap : NotifyPropertyChanged
    {
        public Type PropertyType { get; set; }

        private string assignFrom;
        public string AssignFrom
        {
            get => assignFrom;
            set
            {
                assignFrom = value;
                OnPropertyChanged();
            }
        }

        private string assignTo;
        public string AssignTo
        {
            get => assignTo;
            set
            {
                assignTo = value;
                OnPropertyChanged();
            }
        }
    }
}
