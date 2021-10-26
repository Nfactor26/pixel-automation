using System;

namespace Pixel.Automation.Core.TestData
{
    public enum TestStatus
    {
        None,
        Success,
        Failed,
        Aborted        
    }

    /// <summary>
    /// Result of execution of a <see cref="TestCase"/>
    /// </summary>
    public class TestResult : NotifyPropertyChanged
    {

        public static TestResult EmptyResult { get; } = new TestResult() { Result = TestStatus.None };

        TestStatus result = TestStatus.None;
        /// <summary>
        /// Indicates result status i.e. succes, failed, etc.
        /// </summary>
        public TestStatus Result
        {
            get => result;
            set
            {
                result = value;
                OnPropertyChanged();
            }
        }

        TimeSpan executionTime = TimeSpan.Zero;
        /// <summary>
        /// Time taken for execution
        /// </summary>
        public TimeSpan ExecutionTime
        {
            get => executionTime;
            set
            {
                executionTime = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Input data for the test case
        /// </summary>
        public string TestData { get; set; }

        /// <summary>
        /// Error message incase there was a failure
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                if(error != null)
                {
                    return error.ToString();
                }
                return string.Empty;
            }
        }


        Exception error;
        /// <summary>
        /// Exception incase there was a failure
        /// </summary>
        public Exception Error
        {
            get => error;
            set
            {
                error = value;
                OnPropertyChanged();
            }
        }

    }
}
