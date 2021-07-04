using Pixel.Persistence.Core.Enums;
using Pixel.Persistence.Core.Models;
using System;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Request
{
    [DataContract]
    public class TestResultRequest
    {
        /// <summary>
        /// Max number of items to retrieve at a time
        /// </summary>
        private readonly int maxPageSize = 50;

        /// <summary>
        /// Get all the test results in a given SessionId
        /// </summary>
        [DataMember(IsRequired = false)]
        public string SessionId { get; set; }

        /// <summary>
        /// Get all the test results belonging to given ProjectId
        /// </summary>
        [DataMember(IsRequired = false)]
        public string ProjectId { get; set; }

        /// <summary>
        /// Get all the test results belonging to given TestId
        /// </summary>
        [DataMember(IsRequired = false)]
        public string TestId { get; set; }


        /// <summary>
        /// Get all the test results with FixtureName equal to specified value
        /// </summary>
        [DataMember(IsRequired = false)]
        public string FixtureName { get; set; }
      
        /// <summary>
        /// Get all the test results having specified result ( passed or failed ).
        /// If no value is specified, tests having any result is retrieved
        /// </summary>
        [DataMember(IsRequired = true)]
        public TestStatus Result { get; set; } 

        /// <summary>
        /// Get all the test results where test is executed after specified date 
        /// </summary>
        [DataMember(IsRequired = false)]
        public DateTime ExecutedAfter { get; set; }

        /// <summary>
        /// Get all the test results where execution time is greater then or equal to specified value in seconds
        /// </summary>
        [DataMember(IsRequired = false)]
        public double ExecutionTimeGte { get; set; }

        /// <summary>
        /// Get all the test results where execution time is less then or equal to specified value in seconds
        /// </summary>
        [DataMember(IsRequired = false)]
        public double ExecutionTimeLte { get; set; }

        /// <summary>
        /// Result is sorted in ascending order for given column name
        /// </summary>
        [DataMember(IsRequired = true)]
        public string SortBy { get; set; } = nameof(TestResult.ExecutionOrder);

        [DataMember(IsRequired = true)]
        public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

        /// <summary>
        /// Page number for the Test results
        /// </summary>
        private int currentPage;
        [DataMember(IsRequired = true)]
        public int CurrentPage
        {
            get => currentPage;
            set
            {
                if (value >= 1)
                {
                    currentPage = value;
                }
            }
        }

        /// <summary>
        /// Number of items to return per page up to maxPageSize (50)
        /// </summary>
        private int pagesSize = 10;
        [DataMember(IsRequired = true)]
        public int PageSize
        {
            get
            {
                return pagesSize;
            }
            set
            {
                pagesSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }

        [IgnoreDataMember]
        public int Skip => (CurrentPage - 1) * PageSize;

        [IgnoreDataMember]
        public int Take => PageSize;

        /// <summary>
        /// Default constructor
        /// </summary>
        public TestResultRequest()
        {
            ExecutedAfter = DateTime.Now.Subtract(TimeSpan.FromDays(30));
            ExecutionTimeGte = 0;
            ExecutionTimeLte = 100;           
        }
       
    }
}
