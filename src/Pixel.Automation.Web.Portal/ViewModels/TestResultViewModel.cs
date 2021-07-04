using Pixel.Persistence.Core.Enums;
using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;

namespace Pixel.Automation.Web.Portal.ViewModels
{
    public class TestResultViewModel
    {
        private TestResult testResult;

        public string Id => testResult.Id;

        public string SessionId => testResult.SessionId;

        public string TestId => testResult.TestId;

        public string FixtureName => testResult.FixtureName;

        public string TestName => testResult.TestName;

        public int ExecutionOrder => testResult.ExecutionOrder;

        public double ExecutionTime => testResult.ExecutionTime;

        public DateTime ExecutedOn => testResult.ExecutedOn.ToLocalTime();

        public TestStatus Result => testResult.Result;

        public FailureDetailsViewModel ErrorDetails { get; private set; }

        public bool IsErrorDetailsVisible { get; set; } = false;

        public TestResultViewModel(TestResult testResult)
        {
            this.testResult = testResult;
            if(testResult.FailureDetails != null)
            {
                this.ErrorDetails = new FailureDetailsViewModel(testResult.SessionId, testResult.TestId, testResult.FailureDetails);
            }
        }      
    }

    public static class TestResultViewModelExtension
    {
        public static IEnumerable<TestResultViewModel> ToViewModel(this IEnumerable<TestResult> testResults)
        {
            foreach (var testResult in testResults)
            {
                yield return new TestResultViewModel(testResult);
            }

        }
    }
}
