using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.TestData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    public class TestCaseViewModel : NotifyPropertyChanged
    {
        public TestCase TestCase { get; }    

        public TestCaseViewModel(TestCase testCase)
        {
            this.TestCase = testCase;
        }

        public string Id
        {
            get => TestCase.Id;
            set => TestCase.Id = value;
        }

        public string FixtureId
        {
            get => TestCase.FixtureId;
            set => TestCase.FixtureId = value;
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

        public IEnumerable<string> Tags
        {
            get => TestCase.Tags;
            set => TestCase.Tags = value;
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
        public int Order

        {
            get => TestCase.Order;
            set => TestCase.Order = value;

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

        [NonSerialized]
        ObservableCollection<TestResult> testResults = new ObservableCollection<TestResult>();
        public ObservableCollection<TestResult> TestResults
        {
            get => testResults;
            set => testResults = value;
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


        public void UpdateVisibility(string filterText)
        {
            IsVisible = string.IsNullOrEmpty(filterText) ? true :  this.DisplayName.ToLower().Contains(filterText.ToLower());
        }
    }
}
