using Pixel.Automation.Core;
using Pixel.Automation.Core.TestData;
using System.Collections.ObjectModel;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    public class TestCategoryViewModel : NotifyPropertyChanged
    {       
        public TestCategory TestCategory { get; }

        public ObservableCollection<TestCaseViewModel> Tests { get; set; } = new ObservableCollection<TestCaseViewModel>();


        public string Id
        {
            get => TestCategory.Id;
        }

        public string DisplayName
        {
            get => TestCategory.DisplayName;
            set => TestCategory.DisplayName = value;
        }

        public string Description
        {
            get => TestCategory.Description;
            set => TestCategory.Description = value;
        }


        public bool IsOrdered
        {
            get => TestCategory.IsOrdered;
            set => TestCategory.IsOrdered = value;
        }

        public bool IsMuted
        {
            get => TestCategory.IsMuted;
            set => TestCategory.IsMuted = value;
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

        public TestCategoryViewModel(TestCategory testCategory)
        {
            this.TestCategory = testCategory;
        }
    
    }
}
