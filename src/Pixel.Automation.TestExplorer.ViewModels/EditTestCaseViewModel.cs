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
    /// <summary>
    /// View Model used for editing the details of a <see cref="TestCase"/>
    /// </summary>
    public class EditTestCaseViewModel : SmartScreen
    {
        private readonly ILogger logger = Log.ForContext<EditTestCaseViewModel>();
        private readonly TestCaseViewModel testCase;
        private readonly IEnumerable<string> existingTestCases;

        /// <summary>
        /// A clone of TestCaseViewModel where edits are made.
        /// Actual copy will be replaced with edited copy on user confirmation after edit is done.
        /// </summary>
        public TestCaseViewModel CopyOfTestCase { get; }

        /// <summary>
        /// DisplayName for the TestCase
        /// </summary>
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

        /// <summary>
        /// Description for the TestCase
        /// </summary>
        public string TestCaseDescrpition
        {
            get => CopyOfTestCase.Description;
            set
            {
                CopyOfTestCase.Description = value;
                NotifyOfPropertyChange(() => TestCaseDescrpition);
            }
        }

        /// <summary>
        /// Indicates if TestCase is muted.
        /// A muted TestCase is not executed.
        /// </summary>
        public bool IsMuted
        {
            get => CopyOfTestCase.IsMuted;
            set => CopyOfTestCase.IsMuted = value;
        }

        /// <summary>
        /// Controls the execution speed of the TestCase
        /// </summary>
        public int DelayFactor
        {
            get => (100 - CopyOfTestCase.DelayFactor);
            set => CopyOfTestCase.DelayFactor = (100 - value);
        }

        /// <summary>
        /// Order of the TestCase withing a TestFixture.
        /// Order determines the order of execution of TestCase
        /// </summary>
        public int Order
        {
            get => CopyOfTestCase.Order;
            set => CopyOfTestCase.Order = value;
        }

        /// <summary>
        /// <see cref="Priority"/> for a TestCase.
        /// This can be used to query the TestCases to be executed and doesn't have any significance
        /// during the actual execution of TestCase
        /// </summary>
        public Priority Priority
        {
            get => CopyOfTestCase.Priority;
            set => CopyOfTestCase.Priority = value;
        }

        /// <summary>
        /// A collection of (key,value) tags associated with TestCase. This can be used to filter the TestCases to be executed
        /// and doesn't have any significance during the actual execution of TestCase.
        /// </summary>
        public TagCollectionViewModel TagCollection { get; private set; } = new TagCollectionViewModel();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="testCaseVM"></param>
        /// <param name="existingTestCases"></param>
        public EditTestCaseViewModel(TestCaseViewModel testCaseVM, IEnumerable<TestCaseViewModel> existingTestCases)
        {
            this.testCase = testCaseVM;
            this.existingTestCases = existingTestCases.Select(s => s.DisplayName);
            this.CopyOfTestCase = new TestCaseViewModel(testCaseVM.TestCase.Clone() as TestCase);
            foreach (var tag in testCaseVM.Tags)
            {
                this.TagCollection.Add(new TagViewModel(tag));
            }
        }

        /// <summary>
        /// Apply the edits done of temporary copy to the actual instance of TestCase
        /// and close the screen.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Close the screen without applying any edits done.
        /// </summary>
        /// <returns></returns>
        public async Task Cancel()
        {
            await this.TryCloseAsync(false);
            logger.Information("Edit Test Case changes were cancelled.");
        }

        #region Validation

        /// <inheritdoc/>
        public override bool ShowModelErrors => HasErrors && propertyErrors.ContainsKey(nameof(TagCollection)) && propertyErrors[nameof(TagCollection)].Count() > 0;

        /// <inheritdoc/>
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
