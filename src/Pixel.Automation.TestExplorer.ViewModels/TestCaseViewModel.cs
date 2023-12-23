using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.TestData;
using System;
using System.Collections.ObjectModel;
using System.Linq;

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
        public TestCase TestCase { get; private set; }    

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
        /// Amount of delay in ms after each actor is executed.      
        /// </summary>
        public int PostDelay
        {
            get => TestCase.PostDelay;
            set => TestCase.PostDelay = value;
        }

        /// <summary>
        /// Scaling factor for post delay amount.
        /// This will eventually control the delay introduced between execution of each actor.
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
        /// Indicates if there is a pending change
        /// </summary>
        public bool IsDirty { get; set; } = false;

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
        /// Replace the underlying TestCase model with a new instance.
        /// This is required when a new copy of TestCase might be available but we want to reuse the view model.
        /// </summary>
        /// <param name="testCase"></param>
        public void WithTestCase(TestCase testCase)
        {
            Guard.Argument(testCase, nameof(TestCase)).NotNull();
            this.TestCase = testCase;
        }

        /// <summary>
        /// Associate usage of control to the test case by storing the identifier of control
        /// </summary>
        /// <param name="prefabId"></param>
        public bool AddControlUsage(string controlId)
        {
            bool wasUsagedAddedd = false;
            if (this.TestCase.ControlsUsed.Any(a => a.ControlId.Equals(controlId)))
            {
                var entry = this.TestCase.ControlsUsed.First(a => a.ControlId.Equals(controlId));
                entry.Count++;
            }
            else
            {
                this.TestCase.ControlsUsed.Add(new Core.Models.ControlUsage() { ControlId = controlId, Count = 1 });
                wasUsagedAddedd = true;
            }
            this.IsDirty = true;
            return wasUsagedAddedd;
        }

        /// <summary>
        /// Remove usage of control from the test case
        /// </summary>
        /// <param name="controlId"></param>
        public bool RemoveControlUsage(string controlId)
        {
            bool wasUsagedRemoved = false;
            if (this.TestCase.ControlsUsed.Any(a => a.ControlId.Equals(controlId)))
            {
                var entry = this.TestCase.ControlsUsed.First(a => a.ControlId.Equals(controlId));
                if (entry.Count > 1)
                {
                    entry.Count--;                   
                }
                else
                {
                    this.TestCase.ControlsUsed.Remove(entry);
                    wasUsagedRemoved = true;
                }
                this.IsDirty = true;
            }
            return wasUsagedRemoved;
        }

        /// <summary>
        /// Associate usage of prefab to the test cse by storing the identifier of prefab
        /// </summary>
        /// <param name="prefabId"></param>
        public bool AddPrefabUsage(string prefabId)
        {
            bool wasUsagedAddedd = false;
            if (this.TestCase.PrefabsUsed.Any(a => a.PrefabId.Equals(prefabId)))
            {
                var entry = this.TestCase.PrefabsUsed.First(a => a.PrefabId.Equals(prefabId));
                entry.Count++;
            }
            else
            {
                this.TestCase.PrefabsUsed.Add(new Core.Models.PrefabUsage() { PrefabId = prefabId, Count = 1 });
                wasUsagedAddedd = true;
            }
            this.IsDirty = true;
            return wasUsagedAddedd;
        }

        /// <summary>
        /// Remove usage of prefab from the test case
        /// </summary>
        /// <param name="prefabId"></param>
        public bool RemovePrefabUsage(string prefabId)
        {
            bool wasUsagedRemoved = false;
            if (this.TestCase.PrefabsUsed.Any(a => a.PrefabId.Equals(prefabId)))
            {
                var entry = this.TestCase.PrefabsUsed.First(a => a.PrefabId.Equals(prefabId));
                if (entry.Count > 1)
                {
                    entry.Count--;                   
                }
                else
                {
                    this.TestCase.PrefabsUsed.Remove(entry);
                    wasUsagedRemoved = true;
                }
                this.IsDirty = true;
            }       
            return wasUsagedRemoved;
        }

        /// <summary>
        /// Determine whether this TestCase should be visible on the TestExplorer
        /// </summary>
        /// <param name="filterText"></param>
        public void UpdateVisibility(string filterText)
        {
            if(string.IsNullOrEmpty(filterText))
            {
                IsVisible = true;
                return;
            }

            string[] query = filterText.Split(new char[] { ':' });
            foreach(var part in query)
            {
                if(string.IsNullOrEmpty(part))
                {
                    IsVisible = true;
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
                        case "prefab":
                            IsVisible = this.TestCase.PrefabsUsed.Any(p => p.PrefabId.Equals(query[1]));
                            break;
                        case "control":
                            IsVisible = this.TestCase.ControlsUsed.Any(p => p.ControlId.Equals(query[1]));
                            break;
                        case "testdatasource":
                            IsVisible = this.TestDataId.Equals(query[1]);
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
