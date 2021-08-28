using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    /// <summary>
    /// View Model used for editing the details of a <see cref="TestFixture"/>
    /// </summary>
    public class EditTestFixtureViewModel : SmartScreen
    {
        private readonly ILogger logger = Log.ForContext<EditTestFixtureViewModel>();
        private readonly TestFixture testFixture;
        private readonly IEnumerable<string> existingFixtures;

        /// <summary>
        /// A clone of TestFixtureViewModel where edits are made.
        /// Actual copy will be replaced with edited copy on user confirmation after edit is done.
        /// </summary>
        public TestFixture CopyOfTestFixture { get; }

        /// <summary>
        /// DisplayName for the Fixture
        /// </summary>
        public string TestFixtureDisplayName
        {
            get => CopyOfTestFixture.DisplayName;
            set
            {
                CopyOfTestFixture.DisplayName = value;
                ValidateProperty(nameof(TestFixtureDisplayName));
                NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Category to which TestFixture belongs. This can be any user defined value and is used to filter the TestFixtures to be executed
        /// and doesn't have any significance during the actual execution of the TestCases belonging to a TestFixture.
        /// </summary>
        public string TestFixtureCategory
        {
            get => CopyOfTestFixture.Category;
            set
            {
                CopyOfTestFixture.Category = value;
                ValidateProperty(nameof(TestFixtureCategory));
                NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Description for the Fixture
        /// </summary>
        public string TestFixtureDescription
        {
            get => CopyOfTestFixture.Description;
            set
            {
                CopyOfTestFixture.Description = value;
                ValidateProperty(nameof(TestFixtureDescription));
                NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Indicates if the Fixture is muted.
        /// None of the TestCases belonging to a muted fixture will be executed.
        /// </summary>
        public bool IsMuted
        {
            get => CopyOfTestFixture.IsMuted;
            set => CopyOfTestFixture.IsMuted = value;
        }

        /// <summary>
        /// Controls the execution speed of the TestCases belonging to a TestFixture
        /// </summary>
        public int DelayFactor
        {
            get => (100 - CopyOfTestFixture.DelayFactor);
            set => CopyOfTestFixture.DelayFactor = (100 - value);
        }

        /// <summary>
        /// Order of execution of the TestFixture amongst a group of other Fixtures.
        /// </summary>
        public int Order
        {
            get => CopyOfTestFixture.Order;
            set => CopyOfTestFixture.Order = value;
        }

        /// <summary>
        /// A collection of (key,value) tags associated with TestFixture. This can be used to filter the TestFixtures to be executed
        /// and doesn't have any significance during the actual execution of TestCases belonging to a TestFixture.
        /// </summary>
        public TagCollectionViewModel TagCollection { get; } = new TagCollectionViewModel();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="testFixtureVM"></param>
        /// <param name="existingFixtures"></param>
        public EditTestFixtureViewModel(TestFixture testFixture, IEnumerable<string> existingFixtures)
        {
            this.testFixture = testFixture;
            this.existingFixtures = existingFixtures;
            this.CopyOfTestFixture = testFixture.Clone() as TestFixture;
            foreach (var tag in testFixture.Tags)
            {
                this.TagCollection.Add(new TagViewModel(tag));
            }
        }

        /// <summary>
        /// Apply the edits done of temporary copy to the actual instance of TestFixture
        /// and close the screen.
        /// </summary>
        /// <returns></returns>
        public async Task Save()
        {
            try
            {
                if (Validate())
                {
                    this.testFixture.DisplayName = CopyOfTestFixture.DisplayName;
                    this.testFixture.Description = CopyOfTestFixture.Description;
                    this.testFixture.Category = CopyOfTestFixture.Category;
                    this.testFixture.IsMuted = CopyOfTestFixture.IsMuted;
                    this.testFixture.Order = CopyOfTestFixture.Order;
                    this.testFixture.DelayFactor = CopyOfTestFixture.DelayFactor;
                    this.testFixture.Tags.Clear();
                    foreach (var item in this.TagCollection.Tags)
                    {
                        if (!item.IsDeleted)
                        {
                            this.testFixture.Tags.Add(item.Key, item.Value);
                        }
                    }
                    await this.TryCloseAsync(true);
                    logger.Information("Edit Test Fixture changes applied.");
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
            logger.Information("Edit Test Fixture changes were cancelled.");
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
            ValidateProperty(nameof(TestFixtureDisplayName));
            ValidateProperty(nameof(TestFixtureCategory));
            ValidateProperty(nameof(TagCollection));
            return !HasErrors;
        }

        private void ValidateProperty(string propertyName)
        {
            ClearErrors(propertyName);
            switch (propertyName)
            {
                case nameof(TestFixtureDisplayName):
                    ValidateRequiredProperty(nameof(TestFixtureDisplayName), TestFixtureDisplayName);
                    if (this.existingFixtures.Any(a => a.Equals(TestFixtureDisplayName)))
                    {
                        AddOrAppendErrors(nameof(TestFixtureDisplayName), "Name must be unique.");
                    }
                    break;
                case nameof(TestFixtureCategory):
                    ValidateRequiredProperty(nameof(TestFixtureCategory), TestFixtureCategory);
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
