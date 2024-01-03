using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    /// <summary>
    /// View Model for <see cref="TestFixture"/>
    /// </summary>
    public class TestFixtureViewModel : NotifyPropertyChanged
    {
        /// <summary>
        /// Underlying Model for the view
        /// </summary>
        public TestFixture TestFixture { get; private set; }

        private readonly ObservableCollection<TestCaseViewModel> tests = new ObservableCollection<TestCaseViewModel>();
        /// <summary>
        /// A colllection of TestCases beloning to this TestFixture
        /// </summary>
        public ReadOnlyObservableCollection<TestCaseViewModel> Tests { get; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="testFixture"></param>
        public TestFixtureViewModel(TestFixture testFixture)
        {
            this.TestFixture = testFixture;
            this.Tests = new ReadOnlyObservableCollection<TestCaseViewModel>(tests);
        }

        /// <summary>
        /// Identifier for the Fixture
        /// </summary>
        public string FixtureId
        {
            get => TestFixture.FixtureId;           
        }       

        /// <summary>
        /// DisplayName for the Fixture
        /// </summary>
        public string DisplayName
        {
            get => TestFixture.DisplayName;
            set
            {
                TestFixture.DisplayName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Description for the Fixture
        /// </summary>
        public string Description
        {
            get => TestFixture.Description;
            set => TestFixture.Description = value;
        }

        /// <summary>
        /// Indicates if the Fixture is muted.
        /// None of the TestCases belonging to a muted fixture will be executed.
        /// </summary>
        public bool IsMuted
        {
            get => TestFixture.IsMuted;
            set
            {
                TestFixture.IsMuted = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Order of execution of the TestFixture amongst a group of other Fixtures.
        /// </summary>
        public int Order
        {
            get => TestFixture.Order;
            set
            {
                TestFixture.Order = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// A collection of (key,value) tags associated with TestFixture. This can be used to filter the TestFixtures to be executed
        /// and doesn't have any significance during the actual execution of TestCases belonging to a TestFixture.
        /// </summary>
        public TagCollection Tags
        {
            get => TestFixture.Tags;         
        }

        /// <summary>
        /// Category to which TestFixture belongs. This can be any user defined value and is used to filter the TestFixtures to be executed
        /// and doesn't have any significance during the actual execution of the TestCases belonging to a TestFixture.
        /// </summary>
        public string Category
        {
            get => TestFixture.Category;
            set => TestFixture.Category = value;
        }

        /// <summary>
        /// Amount of delay in ms after each actor is executed.      
        /// </summary>
        public int PostDelay
        {
            get => TestFixture.PostDelay;
            set => TestFixture.PostDelay = value;
        }

        /// <summary>
        /// Scaling factor for post delay amount.      
        /// </summary>
        public int DelayFactor
        {
            get => TestFixture.DelayFactor;
            set => TestFixture.DelayFactor = value;
        }
        
        /// <summary>
        /// TestFixtureEntity
        /// </summary>
        public Entity TestFixtureEntity
        {
            get => TestFixture.TestFixtureEntity;
            set => TestFixture.TestFixtureEntity = value;
        }

        /// <summary>
        /// Initialization Script file for a TestFixture
        /// </summary>
        public string ScriptFile
        {
            get => TestFixture.ScriptFile;
            set => TestFixture.ScriptFile = value;
        }

        /// <summary>
        /// Number of test cases that belongs to the fixture
        /// </summary>
        public int NumberOfTestCases
        {
            get => this.TestFixture.TestCases.Count;
        }

        bool isOpenForEdit;
        /// <summary>
        /// Indicates if the TestFixture is open for edit
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
        /// Guard method to determine whether the TestFixture can be opened for edit
        /// </summary>
        public bool CanOpenForEdit
        {
            get => !isOpenForEdit;
        }

        bool isSelected;
        /// <summary>
        /// Indicates if the TestFixture is currently selected in TestExplorer
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

        bool isVisible = true;
        /// <summary>
        /// Indicates whether the TestFixture is visible in TestExplorer.
        /// This is controlled by the filter applied on TestExplorer
        /// </summary>
        public bool IsVisible
        {
            get => isVisible || this.Tests.Any(t => t.IsVisible);
            private set
            {
                isVisible = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Indicates if the test cases have been loaded for the fixture.
        /// </summary>
        public bool IsLoaded
        {
            get => this.Tests.Any() || (!this.Tests.Any() && !this.TestFixture.TestCases.Any());
        }

        /// <summary>
        /// Indicates whether the TestFixture is being opened for execution.
        /// If true, skip setting up editors, etc which is required only when opened for edit.
        /// </summary>
        public bool OpenForExecute { get; set; } = false;

        /// <summary>
        /// Indicates if there is a pending change
        /// </summary>
        public bool IsDirty { get; set; } = false;

        /// <summary>
        /// Replace the underlying TestFixture model with a new instance.
        /// This is required when a new copy of TestFixture might be available but we want to reuse the view model.
        /// </summary>
        /// <param name="testFixture"></param>
        public void WithTestFixture(TestFixture testFixture)
        {
            Guard.Argument(testFixture, nameof(testFixture)).NotNull();
            this.TestFixture = testFixture;
        }

        /// <summary>
        /// Check if given test case identifier belongs to the fixture
        /// </summary>
        /// <param name="testCaseId"></param>
        /// <returns></returns>
        public bool HasTestCase(string testCaseId)
        {
            return this.TestFixture.TestCases.Contains(testCaseId);
        }

        /// <summary>
        /// Clear the Test Cases view model collection
        /// </summary>
        /// <returns></returns>
        public void ClearTestCases()
        {
            this.tests.Clear();
        }

        /// <summary>
        /// Add a new test case to the fixture
        /// </summary>
        /// <param name="testCase"></param>
        public void AddTestCase(TestCaseViewModel testCase, IProjectFileSystem projectFileSystem)
        {
            if(!this.Tests.Contains(testCase))
            {
                this.tests.Add(testCase);               
            }         
            if(!this.TestFixture.TestCases.Contains(testCase.TestCaseId))
            {
                this.TestFixture.TestCases.Add(testCase.TestCaseId);
                var fixtureFiles = projectFileSystem.GetTestFixtureFiles(this.TestFixture);
                projectFileSystem.SaveToFile<TestFixture>(this.TestFixture, fixtureFiles.FixtureDirectory, Path.GetFileName(fixtureFiles.FixtureFile));
            }    
        }

        /// <summary>
        /// Delete an existing test case from the fixture
        /// </summary>
        /// <param name="testCase"></param>
        public void DeleteTestCase(TestCaseViewModel testCase, IProjectFileSystem projectFileSystem)
        {
            if (this.Tests.Contains(testCase))
            {
                this.tests.Remove(testCase);
                this.TestFixture.TestCases.Remove(testCase.TestCaseId);
                var fixtureFiles = projectFileSystem.GetTestFixtureFiles(this.TestFixture);
                projectFileSystem.SaveToFile<TestFixture>(this.TestFixture, fixtureFiles.FixtureDirectory, Path.GetFileName(fixtureFiles.FixtureFile));
            }
        }

        /// <summary>
        /// Associate usage of control to the test fixture by storing the identifier of control
        /// </summary>
        /// <param name="prefabId"></param>
        public bool AddControlUsage(string controlId)
        {
            bool wasUsageAdded = false;
            if (this.TestFixture.ControlsUsed.Any(a => a.ControlId.Equals(controlId)))
            {
                var entry = this.TestFixture.ControlsUsed.First(a => a.ControlId.Equals(controlId));
                entry.Count++;
            }
            else
            {
                this.TestFixture.ControlsUsed.Add(new Core.Models.ControlUsage() { ControlId = controlId, Count = 1 });
                wasUsageAdded = true;
            }
            this.IsDirty = true;
            return wasUsageAdded;
        }

        /// <summary>
        /// Remove usage of control from the test fixture
        /// </summary>
        /// <param name="controlId"></param>
        public bool RemoveControlUsage(string controlId)
        {
            bool wasUsageRemoved = false;
            if (this.TestFixture.ControlsUsed.Any(a => a.ControlId.Equals(controlId)))
            {
                var entry = this.TestFixture.ControlsUsed.First(a => a.ControlId.Equals(controlId));
                if (entry.Count > 1)
                {
                    entry.Count--;                    
                }
                else
                {
                    this.TestFixture.ControlsUsed.Remove(entry);
                    wasUsageRemoved = true;
                }
                this.IsDirty = true;              
            }
            return wasUsageRemoved;
        }

        /// <summary>
        /// Associate usage of prefab to the test fixture by storing the identifier of prefab
        /// </summary>
        /// <param name="prefabId"></param>
        public bool AddPrefabUsage(string prefabId)
        {
            bool wasUsageAdded = false;
            if (this.TestFixture.PrefabsUsed.Any(a => a.PrefabId.Equals(prefabId)))
            {
                var entry = this.TestFixture.PrefabsUsed.First(a => a.PrefabId.Equals(prefabId));
                entry.Count++;
            }
            else
            {
                this.TestFixture.PrefabsUsed.Add(new Core.Models.PrefabUsage() { PrefabId = prefabId, Count = 1 });
                wasUsageAdded = true;
            }
            this.IsDirty = true;
            return wasUsageAdded;
        }

        /// <summary>
        /// Remove usage of prefab from the test fixture
        /// </summary>
        /// <param name="prefabId"></param>
        public bool RemovePrefabUsage(string prefabId)
        {
            bool wasUsageRemvoed = false;
            if (this.TestFixture.PrefabsUsed.Any(a => a.PrefabId.Equals(prefabId)))
            {
                var entry = this.TestFixture.PrefabsUsed.First(a => a.PrefabId.Equals(prefabId));
                if(entry.Count > 1)
                {
                    entry.Count--;                   
                }
                else
                {
                    this.TestFixture.PrefabsUsed.Remove(entry);
                    wasUsageRemvoed = true;
                }
                this.IsDirty = true;
            }
            return wasUsageRemvoed;
        }

        /// <summary>
        /// Determine whether this TestFixture should be visible on the TestExplorer
        /// </summary>
        /// <param name="filterText"></param>
        public void UpdateVisibility(string filterText)
        {
            if (string.IsNullOrEmpty(filterText))
            {
                IsVisible = true;
                return;
            }

            string[] query = filterText.Split(new char[] { ':' });
            foreach (var part in query)
            {
                if (string.IsNullOrEmpty(part))
                {
                    IsVisible = true;
                    return;
                }
            }
            switch (query.Length)
            {
                case 1:
                    IsVisible = this.DisplayName.ToLower().Contains(filterText.ToLower());
                    break;
                case 2: // we have a key value pair query
                    switch (query[0].ToLower())
                    {
                        case "name":
                            IsVisible = this.DisplayName.ToLower().Contains(query[1]);
                            break;
                        case "prefab":
                            IsVisible = this.TestFixture.PrefabsUsed.Any(p => p.PrefabId.Equals(query[1]));
                            break;
                        case "control":
                            IsVisible = this.TestFixture.ControlsUsed.Any(p => p.ControlId.Equals(query[1]));
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
