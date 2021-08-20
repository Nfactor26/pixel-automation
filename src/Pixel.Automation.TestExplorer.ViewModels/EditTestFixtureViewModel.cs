using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    public class EditTestFixtureViewModel : SmartScreen
    {
        private readonly ILogger logger = Log.ForContext<EditTestFixtureViewModel>();
        private readonly TestFixtureViewModel testFixtureVM;
        private readonly IEnumerable<string> existingFixtures;

        public TestFixtureViewModel CopyOfTestFixture { get; }

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

        public bool IsMuted
        {
            get => CopyOfTestFixture.IsMuted;
            set => CopyOfTestFixture.IsMuted = value;
        }

        public int DelayFactor
        {
            get => (100 - CopyOfTestFixture.DelayFactor);
            set => CopyOfTestFixture.DelayFactor = (100 - value);
        }

        public int Order
        {
            get => CopyOfTestFixture.Order;
            set => CopyOfTestFixture.Order = value;
        }
        public TagCollectionViewModel TagCollection { get; private set; } = new TagCollectionViewModel();

        public EditTestFixtureViewModel(TestFixtureViewModel testFixtureVM, IEnumerable<TestFixtureViewModel> existingFixtures)
        {
            this.testFixtureVM = testFixtureVM;
            this.existingFixtures = existingFixtures.Select(s => s.DisplayName);
            this.CopyOfTestFixture = new TestFixtureViewModel(testFixtureVM.TestFixture.Clone() as TestFixture);
            foreach (var tag in testFixtureVM.Tags)
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
                    this.testFixtureVM.DisplayName = CopyOfTestFixture.DisplayName;
                    this.testFixtureVM.Description = CopyOfTestFixture.Description;
                    this.testFixtureVM.Category = CopyOfTestFixture.Category;
                    this.testFixtureVM.IsMuted = CopyOfTestFixture.IsMuted;
                    this.testFixtureVM.Order = CopyOfTestFixture.Order;
                    this.testFixtureVM.DelayFactor = CopyOfTestFixture.DelayFactor;
                    this.testFixtureVM.Tags.Clear();
                    foreach (var item in this.TagCollection.Tags)
                    {
                        if (!item.IsDeleted)
                        {
                            this.testFixtureVM.Tags.Add(item.Key, item.Value);
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

        public async Task Cancel()
        {
            await this.TryCloseAsync(false);
            logger.Information("Edit Test Fixture changes were cancelled.");
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
