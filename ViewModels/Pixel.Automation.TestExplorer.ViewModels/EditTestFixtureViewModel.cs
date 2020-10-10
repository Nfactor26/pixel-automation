﻿using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    public class EditTestFixtureViewModel : SmartScreen
    {
        private readonly TestFixtureViewModel testFixtureVM;
        private readonly IEnumerable<TestFixtureViewModel> existingFixtures;

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

        public int Order
        {
            get => CopyOfTestFixture.Order;
            set => CopyOfTestFixture.Order = value;
        }

        public string Tags
        {
            get => string.Join(",", CopyOfTestFixture.Tags);
            set
            {
                CopyOfTestFixture.Tags = value.Split(new char[] { ',' });
                NotifyOfPropertyChange();
            }
        }


        public string Group
        {
            get => CopyOfTestFixture.Group;
            set
            {
                CopyOfTestFixture.Group = value;
                ValidateProperty(nameof(Group));
                NotifyOfPropertyChange();
            }
        }

        public EditTestFixtureViewModel(TestFixtureViewModel testFixtureVM, IEnumerable<TestFixtureViewModel> existingFixtures)
        {
            this.testFixtureVM = testFixtureVM;
            this.existingFixtures = existingFixtures;
            this.CopyOfTestFixture = new TestFixtureViewModel(testFixtureVM.TestFixture.Clone() as TestFixture);
        }

        public bool CanSave
        {
            get => !HasErrors;
        }

        public async void Save()
        {
            if(Validate())
            {
                this.testFixtureVM.DisplayName = CopyOfTestFixture.DisplayName;
                this.testFixtureVM.Description = CopyOfTestFixture.Description;
                this.testFixtureVM.Group = CopyOfTestFixture.Group;
                this.testFixtureVM.IsMuted = CopyOfTestFixture.IsMuted;
                this.testFixtureVM.Order = CopyOfTestFixture.Order;
                this.testFixtureVM.Tags = CopyOfTestFixture.Tags;
                await this.TryCloseAsync(true);
            }           
        }

        public async void Cancel()
        {
            await this.TryCloseAsync(false);
        }

        public bool Validate()
        {
            ValidateProperty(nameof(TestFixtureDisplayName));
            return !HasErrors;
        }

        private void ValidateProperty(string propertyName)
        {          
            ClearErrors(propertyName);
            switch (propertyName)
            {
                case nameof(TestFixtureDisplayName):
                    ValidateRequiredProperty(nameof(TestFixtureDisplayName), TestFixtureDisplayName);
                    if (this.existingFixtures.Any(a => a.DisplayName.Equals(TestFixtureDisplayName)))
                    {
                        AddOrAppendErrors(nameof(TestFixtureDisplayName), "Name must be unique.");
                    }
                    break;
                case nameof(Group):
                    ValidateRequiredProperty(nameof(Group), Group);
                    break;
            }        
            NotifyOfPropertyChange(() => CanSave);
        }

    }
}