using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.TestData
{

    [DataContract]
    [Serializable]
    public class TestCase : NotifyPropertyChanged, ICloneable
    {
        [DataMember]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [DataMember]
        public string CategoryId { get; set; }    

        string displayName;
        [DataMember]
        public string DisplayName
        {
            get => displayName;
            set
            {
                displayName = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public IEnumerable<string> Tags { get; set; } = new List<string>();

        bool isMuted;
        [DataMember]
        public bool IsMuted
        {
            get => isMuted;
            set
            {
                isMuted = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public int Order { get; set; }
     
        public Entity TestCaseEntity { get; set; }

        [DataMember]
        public string ScriptFile { get; set; }

        string testDataId;
        /// <summary>
        /// Id of the data source in test data repository
        /// </summary>
        [DataMember(IsRequired = false)]
        public string TestDataId
        {
            get => testDataId;
            set
            {
                testDataId = value;
                OnPropertyChanged();
            }

        }

        [NonSerialized]
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

        [NonSerialized]
        bool isOpenForEdit;
        public bool IsOpenForEdit
        {
            get => isOpenForEdit;
            set
            {
                isOpenForEdit = value;
                OnPropertyChanged();
            }
        }

        [NonSerialized]
        bool isRunning;
        public bool IsRunning
        {
            get => isRunning;
            set
            {
                isRunning = value;
                OnPropertyChanged();
            }
        }

        [NonSerialized]
        ObservableCollection<TestResult> testResults = new ObservableCollection<TestResult>();
        public ObservableCollection<TestResult> TestResults
        {
            get => testResults;
            set
            {
                testResults = value;              
            }
        }

        public object Clone()
        {
            TestCase copy = new TestCase()
            {
                Id = this.Id,
                DisplayName = this.DisplayName,
                Description = this.Description,   
                Tags = this.Tags,
                IsMuted = this.IsMuted,               
                Order = this.Order,
                TestCaseEntity = this.TestCaseEntity                
            };
            return copy;
        }
    }

}
