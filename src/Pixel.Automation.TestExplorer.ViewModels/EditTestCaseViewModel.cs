using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    public class EditTestCaseViewModel : SmartScreen
    {
        private readonly ILogger logger = Log.ForContext<EditTestCaseViewModel>();
        private readonly TestCaseViewModel testCase;
        private readonly IEnumerable<string> existingTestCases;

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

        public int DelayFactor
        {
            get => (100 - CopyOfTestCase.DelayFactor);
            set => CopyOfTestCase.DelayFactor = (100 - value);
        }


        public int Order
        {
            get => CopyOfTestCase.Order;
            set => CopyOfTestCase.Order = value;
        }

        public Priority Priority
        {
            get => CopyOfTestCase.Priority;
            set => CopyOfTestCase.Priority = value;
        }

        public TagCollectionViewModel TagCollection { get; private set; } = new TagCollectionViewModel();

        public EditTestCaseViewModel(TestCaseViewModel testCaseVM, IEnumerable<TestCaseViewModel> existingTestCases)
        {
            this.testCase = testCaseVM;
            this.existingTestCases = existingTestCases.Select(s => s.DisplayName);
            this.CopyOfTestCase = new TestCaseViewModel(testCaseVM.TestCase.Clone() as TestCase, null);
            foreach (var tag in testCaseVM.Tags)
            {
                this.TagCollection.Add(new TagViewModel(tag));
            }
        }

        public async Task Save()
        {
            try
            {
                if (Validate())
                {
                    this.testCase.DisplayName = CopyOfTestCase.DisplayName;
                    this.testCase.Description = CopyOfTestCase.Description;
                    this.testCase.IsMuted = CopyOfTestCase.IsMuted;
                    this.testCase.Order = CopyOfTestCase.Order;
                    this.testCase.DelayFactor = CopyOfTestCase.DelayFactor;
                    this.testCase.Tags.Clear();
                    foreach (var item in this.TagCollection.Tags)
                    {
                        if (!item.IsDeleted)
                        {
                            this.testCase.Tags.Add(item.Key, item.Value);
                        }
                    }
                    await this.TryCloseAsync(true);
                    logger.Information("Edit Test Case changes applied.");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);                
            }
        }

        public async Task Cancel()
        {
            await this.TryCloseAsync(false);
            logger.Information("Edit Test Case changes were cancelled.");
        }

        #region Validation

        public override bool ShowModelErrors => HasErrors && propertyErrors.ContainsKey(nameof(TagCollection)) && propertyErrors[nameof(TagCollection)].Count() > 0;

        public override void DismissModelErrors()
        {
            ClearErrors(nameof(TagCollection));
            NotifyOfPropertyChange(() => ShowModelErrors);
        }

        private bool Validate()
        {
            ValidateProperty(nameof(TestCaseDisplayName));
            ValidateProperty(nameof(TagCollection));
            return !HasErrors;
        }

        private void ValidateProperty(string propertyName)
        {
            ClearErrors(propertyName);
            switch (propertyName)
            {
                case nameof(TestCaseDisplayName):
                    ValidateRequiredProperty(nameof(TestCaseDisplayName), TestCaseDisplayName);
                    if (this.existingTestCases.Any(a => a.Equals(TestCaseDisplayName)))
                    {
                        AddOrAppendErrors(nameof(TestCaseDisplayName), "Name must be unique.");
                    }
                    break;
                case nameof(TagCollection):
                    if (!this.TagCollection.Validate(out List<string> validationErrors))
                    {
                        AddOrAppendErrors(nameof(TagCollection), validationErrors);
                    }
                    break;
            }          
        }

        #endregion Validation
    }
}
