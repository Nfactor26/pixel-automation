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

  
    public class TestResult : NotifyPropertyChanged
    {

        public static TestResult EmptyResult { get; } = new TestResult() { Result = TestStatus.None };

        TestStatus result = TestStatus.None;
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
        public TimeSpan ExecutionTime
        {
            get => executionTime;
            set
            {
                executionTime = value;
                OnPropertyChanged();
            }
        }
        
        public string TestData { get; set; }

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
