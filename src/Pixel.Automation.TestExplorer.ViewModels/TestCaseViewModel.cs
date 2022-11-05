using Pixel.Automation.Core;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.TestData;
using System;
using System.Collections.ObjectModel;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    /// <summary>
    /// View Model for <see cref="TestCase"/>
    /// </summary>
    public class TestCaseViewModel : NotifyPropertyChanged
    {    
        /// <summary>
        /// Underlying Model for the View
        /// </summary>
        public TestCase TestCase { get; }    

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="testCase"></param>
        public TestCaseViewModel(TestCase testCase)
        {
            this.TestCase = testCase;           
        }

        /// <summary>
        /// Identifier of the TestCase
        /// </summary>
        public string TestCaseId
        {
            get => TestCase.TestCaseId;           
        }

        /// <summary>
        /// Identifier of the TestFixture to which TestCase belongs
        /// </summary>
        public string FixtureId
        {
            get => TestCase.FixtureId;            
        }

        /// <summary>
        /// DisplayName for the TestCase
        /// </summary>
        public string DisplayName
        {
            get => TestCase.DisplayName;
            set
            {
                TestCase.DisplayName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Description for the TestCase
        /// </summary>
        public string Description
        {
            get => TestCase.Description;
            set => TestCase.Description = value;
        }

        /// <summary>
        /// A collection of (key,value) tags associated with TestCase. This can be used to filter the TestCases to be executed
        /// and doesn't have any significance during the actual execution of TestCase.
        /// </summary>
        public TagCollection Tags
        {
            get => TestCase.Tags;           
        }

        /// <summary>
        /// Order of the TestCase withing a TestFixture.
        /// Order determines the order of execution of TestCase
        /// </summary>
        public int Order
        {
            get => TestCase.Order;
            set
            {
                TestCase.Order = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Indicates if TestCase is muted.
        /// A muted TestCase is not executed.
        /// </summary>
        public bool IsMuted
        {
            get => TestCase.IsMuted;
            set
            {
                TestCase.IsMuted = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Controls the execution speed of the TestCase
        /// </summary>
        public int DelayFactor
        {
            get => TestCase.DelayFactor;
            set => TestCase.DelayFactor = value;
        }

        /// <summary>
        /// <see cref="Priority"/> for a TestCase.
        /// This can be used to query the TestCases to be executed and doesn't have any significance
        /// during the actual execution of TestCase
        /// </summary>
        public Priority Priority
        {
            get => TestCase.Priority;
            set
            {
                TestCase.Priority = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// TestCaseEntity
        /// </summary>
        public Entity TestCaseEntity

        {
            get => TestCase.TestCaseEntity;
            set => TestCase.TestCaseEntity = value;
        }

        /// <summary>
        /// Initialization ScriptFile for the TestCase
        /// </summary>
        public string ScriptFile
        {
            get => TestCase.ScriptFile;
            set => TestCase.ScriptFile = value;
        }

        /// <summary>
        /// Identifier of the <see cref="TestDataSource"/> of the TestCase
        /// </summary>
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
        /// <summary>
        /// Indicates if the TestCase is currently selected in TestExplorer
        /// </summary>
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
        /// <summary>
        /// Indicates if the TestCase is open for edit
        /// </summary>
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

        /// <summary>
        /// Guard method for determining whether the TestCase can be opened for edit
        /// </summary>
        public bool CanOpenForEdit
        {
            get => !isOpenForEdit && !string.IsNullOrEmpty(TestDataId);
        }

        bool isRunning;
        /// <summary>
        /// Indicates if the TestCase is currently being executed
        /// </summary>
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
        /// <summary>
        /// Indicates whether the TestCase is visible in TestExplorer.
        /// This is controlled by the filter applied on TestExplorer
        /// </summary>
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
        /// <summary>
        /// Stores the TestResult from last execution. There can be more then one result based on the
        /// number of records in <see cref="TestDataSource"/>
        /// </summary>
        public ObservableCollection<TestResult> TestResults
        {
            get => testResults;
            set => testResults = value;
        }

        /// <summary>
        /// Indicates whether the TestCase is being opened for execution.
        /// If true, skip setting up editors, etc which is required only when opened for edit.
        /// </summary>
        public bool OpenForExecute { get; set; } = false;


        /// <summary>
        /// Set the TestDataSourdce identifier for this TestCase.
        /// TestDataSource is associated by dragging a TestDataSource to a TestCase
        /// </summary>
        /// <param name="testDataSourceId"></param>
        public void SetTestDataSource(string testDataSourceId)
        {
            this.TestDataId = testDataSourceId;           
        }

        /// <summary>
        /// Determine whether this TestCase should be visible on the TestExplorer
        /// </summary>
        /// <param name="filterText"></param>
        public void UpdateVisibility(string filterText)
        {
            if(string.IsNullOrEmpty(filterText))
            {
                isVisible = true;               
            }

            string[] query = filterText.Split(new char[] { ':' });
            foreach(var part in query)
            {
                if(string.IsNullOrEmpty(part))
                {
                    isVisible = true;
                    return;
                }
            }
            switch(query.Length)
            {
                case 1:
                    IsVisible = this.DisplayName.ToLower().Contains(filterText.ToLower());
                    break;
                case 2: // we have a key value pair query
                    switch(query[0].ToLower())
                    {
                        case "name":
                            IsVisible = this.DisplayName.ToLower().Contains(query[1]);
                            break;                      
                        default:
                            IsVisible = this.Tags.Contains(query[0]) && this.Tags[query[0]].Equals(query[1]);
                            break;
                    }
                    break;
            }          
        }
    }
}
