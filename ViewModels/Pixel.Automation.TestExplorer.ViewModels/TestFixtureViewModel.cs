using Pixel.Automation.Core;
using Pixel.Automation.Core.TestData;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    public class TestFixtureViewModel : NotifyPropertyChanged
    {
        public TestFixture TestFixture { get; }

        public ObservableCollection<TestCaseViewModel> Tests { get; set; } = new ObservableCollection<TestCaseViewModel>();

        public TestFixtureViewModel(TestFixture testFixture)
        {
            this.TestFixture = testFixture;           
        }

        public string Id
        {
            get => TestFixture.Id;           
        }       

        public string DisplayName
        {
            get => TestFixture.DisplayName;
            set
            {
                TestFixture.DisplayName = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => TestFixture.Description;
            set => TestFixture.Description = value;
        }

        public bool IsMuted
        {
            get => TestFixture.IsMuted;
            set
            {
                TestFixture.IsMuted = value;
                OnPropertyChanged();
            }
        }


        public int Order
        {
            get => TestFixture.Order;
            set
            {
                TestFixture.Order = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<string> Tags
        {
            get => TestFixture.Tags;
            set => TestFixture.Tags = value;
        }

        public string Group
        {
            get => TestFixture.Group;
            set => TestFixture.Group = value;
        }
        
        public Entity TestFixtureEntity
        {
            get => TestFixture.TestFixtureEntity;
            set => TestFixture.TestFixtureEntity = value;
        }

        public string ScriptFile
        {
            get => TestFixture.ScriptFile;
            set => TestFixture.ScriptFile = value;
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
            get => !isOpenForEdit;
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

        bool isVisible = true;
        public bool IsVisible
        {
            get => isVisible || this.Tests.Any(t => t.IsVisible);
            private set
            {
                isVisible = value;
                OnPropertyChanged();
            }
        }

        public void UpdateVisibility(string filterText)
        {
            foreach (var test in this.Tests)
            {
                test.UpdateVisibility(filterText);
            }
            IsVisible = string.IsNullOrEmpty(filterText) ? true : this.DisplayName.ToLower().Contains(filterText.ToLower());
        }
    }
}
