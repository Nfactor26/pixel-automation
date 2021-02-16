using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    public class EditTestCaseViewModel : SmartScreen
    {
        private readonly TestCaseViewModel testCase;
        private readonly IEnumerable<TestCaseViewModel> existingTestCases;

        public TestCaseViewModel CopyOfTestCase { get; }

        public string TestCaseDisplayName
        {
            get => CopyOfTestCase.DisplayName;
            set
            {
                CopyOfTestCase.DisplayName = value;
                ValidateProperty(nameof(TestCaseDisplayName));
                NotifyOfPropertyChange(() => TestCaseDisplayName);
            }
        }

        public string TestCaseDescrpition
        {
            get => CopyOfTestCase.Description;
            set
            {
                CopyOfTestCase.Description = value;
                NotifyOfPropertyChange(() => TestCaseDescrpition);
            }
        }

        public bool IsMuted
        {
            get => CopyOfTestCase.IsMuted;
            set => CopyOfTestCase.IsMuted = value;
        }

        public int Order
        {
            get => CopyOfTestCase.Order;
            set => CopyOfTestCase.Order = value;
        }

        public string Tags
        {
            get => string.Join(",", CopyOfTestCase.Tags);
            set
            {
                CopyOfTestCase.Tags.Clear();
                CopyOfTestCase.Tags.AddRange(value.Trim(',').Split(new char[] { ',' }));
                NotifyOfPropertyChange();
            }
        }

        public EditTestCaseViewModel(TestCaseViewModel testCaseVM, IEnumerable<TestCaseViewModel> existingTestCases)
        {
            this.testCase = testCaseVM;
            this.existingTestCases = existingTestCases;
            this.CopyOfTestCase = new TestCaseViewModel(testCaseVM.TestCase.Clone() as TestCase, null);
        }

        public bool CanSave
        {
            get => !HasErrors;
        }


        public async void Save()
        {
            if (Validate())
            {
                this.testCase.DisplayName = CopyOfTestCase.DisplayName;
                this.testCase.Description = CopyOfTestCase.Description;
                this.testCase.IsMuted = CopyOfTestCase.IsMuted;
                this.testCase.Order = CopyOfTestCase.Order;
                this.testCase.Tags.Clear();
                this.testCase.Tags.AddRange(CopyOfTestCase.Tags);
                await this.TryCloseAsync(true);
            }
        }

        public async void Cancel()
        {
            await this.TryCloseAsync(false);
        }

        public bool Validate()
        {
            ValidateProperty(nameof(TestCaseDisplayName));
            return !HasErrors;
        }

        private void ValidateProperty(string propertyName)
        {
            ClearErrors(propertyName);
            switch (propertyName)
            {
                case nameof(TestCaseDisplayName):
                    ValidateRequiredProperty(nameof(TestCaseDisplayName), TestCaseDisplayName);
                    if (this.existingTestCases.Any(a => a.DisplayName.Equals(TestCaseDisplayName)))
                    {
                        AddOrAppendErrors(nameof(TestCaseDisplayName), "Name must be unique.");
                    }
                    break;
            }
            NotifyOfPropertyChange(() => CanSave);
        }


    }
}
