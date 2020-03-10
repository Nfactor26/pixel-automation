using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Automation.TestExplorer
{
    public class TestCaseViewModel : SmartScreen
    {
        private readonly TestCase testCase;
        private readonly IEnumerable<TestCase> existingTestCases;

        public TestCase CopyOfTestCase { get; }

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

        public TestCaseViewModel(TestCase testCase, IEnumerable<TestCase> existingTestCases)
        {
            this.testCase = testCase;
            this.existingTestCases = existingTestCases;
            this.CopyOfTestCase = testCase.Clone() as TestCase;
        }


        public bool CanSave
        {
            get => !HasErrors;
        }


        public async void Save()
        {
            if(Validate())
            {
                this.testCase.DisplayName = CopyOfTestCase.DisplayName;
                this.testCase.Description = CopyOfTestCase.Description;
                this.testCase.IsMuted = CopyOfTestCase.IsMuted;
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
                    if(this.existingTestCases.Any(a => a.DisplayName.Equals(TestCaseDisplayName)))
                    {
                       AddOrAppendErrors(nameof(TestCaseDisplayName), "Name must be unique.");
                    }
                    break;                   
            }        
            NotifyOfPropertyChange(() => CanSave);
        }

        
    }
}
