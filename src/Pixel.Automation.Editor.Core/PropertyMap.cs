using Pixel.Automation.Core;
using System;

namespace Pixel.Automation.Editor.Core
{
    public class PropertyMap : NotifyPropertyChanged
    {
        public Type AssignToType { get; set; }

        public Type AssignFromType { get; set; }

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
