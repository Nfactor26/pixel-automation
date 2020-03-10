using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.TestData
{
    [DataContract]
    [Serializable]
    public class TestCategory : NotifyPropertyChanged, ICloneable
    {
        [DataMember]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public bool IsOrdered { get; set; }

        [DataMember]
        public bool IsMuted { get; set; }

        bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TestCase> Tests { get; set; } = new ObservableCollection<TestCase>();

        public object Clone()
        {
            TestCategory copy = new TestCategory()
            {
                Id = this.Id,
                DisplayName = this.DisplayName,
                Description = this.Description,
                IsOrdered = this.IsOrdered,
                IsMuted = this.IsMuted,
                IsSelected = this.IsSelected,
                Tests = this.Tests
            };
            return copy;
        }

    }
}
