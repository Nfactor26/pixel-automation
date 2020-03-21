using Pixel.Automation.Core;
using Pixel.Automation.Core.TestData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    public class TestCaseViewModel : NotifyPropertyChanged
    {
        private readonly TestCase testCase;

        public TestCase TestCase
        {
            get => testCase;
        }

        public TestCaseViewModel(TestCase testCase)
        {
            this.testCase = testCase;
        }

        public string Id
        {
            get => testCase.Id;
            set => testCase.Id = value;
        }

        public string CategoryId
        {
            get => testCase.CategoryId;
            set => testCase.CategoryId = value;
        }

        public string DisplayName
        {
            get => testCase.DisplayName;
            set
            {
                testCase.DisplayName = value;
                OnPropertyChanged();

            }
        }
        public string Description
        {
            get => testCase.Description;
            set => testCase.Description = value;
        }

        public IEnumerable<string> Tags
        {
            get => testCase.Tags;
            set => testCase.Tags = value;
        }

        public bool IsMuted
        {
            get => testCase.IsMuted;
            set
            {
                testCase.IsMuted = value;
                OnPropertyChanged();
            }
        }
        public int Order

        {
            get => testCase.Order;
            set => testCase.Order = value;

        }

        public Entity TestCaseEntity

        {
            get => testCase.TestCaseEntity;
            set => testCase.TestCaseEntity = value;
        }

        public string ScriptFile
        {
            get => testCase.ScriptFile;
            set => testCase.ScriptFile = value;
        }


        public string TestDataId
        {
            get => testCase.TestDataId;
            set
            {
                testCase.TestDataId = value;
                OnPropertyChanged();
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
            }
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

    }
}
