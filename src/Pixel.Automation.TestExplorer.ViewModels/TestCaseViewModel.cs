using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using System;
using System.Collections.ObjectModel;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    public class TestCaseViewModel : NotifyPropertyChanged
    {
        private IEventAggregator eventAggregator;

        public TestCase TestCase { get; }    

        public TestCaseViewModel(TestCase testCase, IEventAggregator eventAggregator)
        {
            this.TestCase = testCase;
            this.eventAggregator = eventAggregator;
        }

        public string Id
        {
            get => TestCase.Id;           
        }

        public string FixtureId
        {
            get => TestCase.FixtureId;            
        }

        public string DisplayName
        {
            get => TestCase.DisplayName;
            set
            {
                TestCase.DisplayName = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => TestCase.Description;
            set => TestCase.Description = value;
        }

        public TagCollection Tags
        {
            get => TestCase.Tags;           
        }


        public int Order
        {
            get => TestCase.Order;
            set
            {
                TestCase.Order = value;
                OnPropertyChanged();
            }
        }

        public bool IsMuted
        {
            get => TestCase.IsMuted;
            set
            {
                TestCase.IsMuted = value;
                OnPropertyChanged();
            }
        }

        public int DelayFactor
        {
            get => TestCase.DelayFactor;
            set => TestCase.DelayFactor = value;
        }

        public Priority Priority
        {
            get => TestCase.Priority;
            set
            {
                TestCase.Priority = value;
                OnPropertyChanged();
            }
        }

        public Entity TestCaseEntity

        {
            get => TestCase.TestCaseEntity;
            set => TestCase.TestCaseEntity = value;
        }

        public string ScriptFile
        {
            get => TestCase.ScriptFile;
            set => TestCase.ScriptFile = value;
        }


        public string TestDataId
        {
            get => TestCase.TestDataId;
            set
            {
                TestCase.TestDataId = value;              
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanOpenForEdit));
            }
        }

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

        bool isOpenForEdit;
        public bool IsOpenForEdit
        {
            get => isOpenForEdit;
            set
            {
                isOpenForEdit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanOpenForEdit));
            }
        }

        public bool CanOpenForEdit
        {
            get => !isOpenForEdit && !string.IsNullOrEmpty(TestDataId);
        }

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

        bool isVisible = true;
        public bool IsVisible
        {
            get => isVisible;
            private set
            {
                isVisible = value;
                OnPropertyChanged();
            }
        }


        [NonSerialized]
        ObservableCollection<TestResult> testResults = new ObservableCollection<TestResult>();
        public ObservableCollection<TestResult> TestResults
        {
            get => testResults;
            set => testResults = value;
        }


        public void SetTestDataSource(string testDataSourceId)
        {
            this.TestDataId = testDataSourceId;
            this.eventAggregator.PublishOnBackgroundThreadAsync(new TestCaseUpdatedEventArgs(this.TestCase));
        }

        public void UpdateVisibility(string filterText)
        {
            IsVisible = string.IsNullOrEmpty(filterText) ? true :  this.DisplayName.ToLower().Contains(filterText.ToLower());
        }
    }
}
