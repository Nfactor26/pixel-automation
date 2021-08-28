using Pixel.Automation.Core;
using Pixel.Automation.Core.TestData;
using System.Collections.ObjectModel;
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
        public TestFixture TestFixture { get; }

        /// <summary>
        /// A colllection of TestCases beloning to this TestFixture
        /// </summary>
        public ObservableCollection<TestCaseViewModel> Tests { get; } = new ObservableCollection<TestCaseViewModel>();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="testFixture"></param>
        public TestFixtureViewModel(TestFixture testFixture)
        {
            this.TestFixture = testFixture;           
        }

        /// <summary>
        /// Identifier for the Fixture
        /// </summary>
        public string Id
        {
            get => TestFixture.Id;           
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
        /// Controls the execution speed of the TestCases belonging to a TestFixture
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
        /// Determine whether this TestFixture should be visible on the TestExplorer
        /// </summary>
        /// <param name="filterText"></param>
        public void UpdateVisibility(string filterText)
        {
            if (string.IsNullOrEmpty(filterText))
            {
                isVisible = true;
            }

            string[] query = filterText.Split(new char[] { ':' });
            foreach (var part in query)
            {
                if (string.IsNullOrEmpty(part))
                {
                    isVisible = true;
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
                        default:
                            IsVisible = this.Tags.Contains(query[0]) && this.Tags[query[0]].Equals(query[1]);
                            break;
                    }
                    break;
            }
            
            foreach (var test in this.Tests)
            {
                test.UpdateVisibility(filterText);
            }         
        }
    }
}
