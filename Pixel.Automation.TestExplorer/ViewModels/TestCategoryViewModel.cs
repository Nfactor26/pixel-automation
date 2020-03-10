using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Automation.TestExplorer
{
    public class TestCategoryViewModel : SmartScreen
    {
        private readonly TestCategory testCategory;
        private readonly IEnumerable<TestCategory> existingCategories;

        public TestCategory CopyOfTestCategory { get; }

        public string TestCategoryDisplayName
        {
            get => CopyOfTestCategory.DisplayName;
            set
            {
                CopyOfTestCategory.DisplayName = value;
                ValidateProperty(nameof(TestCategoryDisplayName));
                NotifyOfPropertyChange(() => TestCategoryDisplayName);
            }
        }

        public string TestCategoryDescription
        {
            get => CopyOfTestCategory.Description;
            set
            {
                CopyOfTestCategory.Description = value;
                ValidateProperty(nameof(TestCategoryDescription));
                NotifyOfPropertyChange(() => TestCategoryDescription);
            }
        }

        public TestCategoryViewModel(TestCategory testCategory, IEnumerable<TestCategory> existingCategories)
        {
            this.testCategory = testCategory;
            this.existingCategories = existingCategories;
            this.CopyOfTestCategory = testCategory.Clone() as TestCategory;
        }

        public bool CanSave
        {
            get => !HasErrors;
        }

        public async void Save()
        {
            if(Validate())
            {
                this.testCategory.DisplayName = CopyOfTestCategory.DisplayName;
                this.testCategory.Description = CopyOfTestCategory.Description;
                this.testCategory.IsMuted = CopyOfTestCategory.IsMuted;
                this.testCategory.IsOrdered = CopyOfTestCategory.IsOrdered;
                await this.TryCloseAsync(true);
            }           
        }

        public async void Cancel()
        {
            await this.TryCloseAsync(false);
        }

        public bool Validate()
        {
            ValidateProperty(nameof(TestCategoryDisplayName));
            return !HasErrors;
        }

        private void ValidateProperty(string propertyName)
        {          
            ClearErrors(propertyName);
            switch (propertyName)
            {
                case nameof(TestCategoryDisplayName):
                    ValidateRequiredProperty(nameof(TestCategoryDisplayName), TestCategoryDisplayName);
                    if (this.existingCategories.Any(a => a.DisplayName.Equals(TestCategoryDisplayName)))
                    {
                        AddOrAppendErrors(nameof(TestCategoryDisplayName), "Name must be unique.");
                    }
                    break;
            }        
            NotifyOfPropertyChange(() => CanSave);
        }

    }
}
