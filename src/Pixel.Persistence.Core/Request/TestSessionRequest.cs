using Pixel.Persistence.Core.Models;
using System;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Request
{
    /// <summary>
    /// Retrieve a collection of <see cref="TestSession"/> from API using TestSessionRequest
    /// TestSessionRequest can be used to configure search criterias like ProjectName, MachineName, etc.
    /// It also has options to specify OrderBy field and number of rows to retrieving (using paging mechanism while specifying 
    /// current page and page size)
    /// </summary>
    [DataContract]
    public class TestSessionRequest
    {
        private readonly int maxPageSize = 50;

        [DataMember(IsRequired = false)]
        public string ProjectName { get; set; }

        [DataMember(IsRequired = false)]
        public string TemplateName { get; set; }

        [DataMember(IsRequired = false)]
        public string MachineName { get; set; }

        [DataMember(IsRequired = true)]
        /// <summary>
        /// By default return sessions executed in current year only
        /// </summary>
        public DateTime ExecutedAfter { get; set; } = new DateTime(DateTime.Now.Year, 1, 1);


        [DataMember(IsRequired = true)]    
        public string OrderBy { get; set; } = nameof(TestSession.SessionStartTime);

        private int currentPage;
        [DataMember(IsRequired = true)]
        public int CurrentPage
        {
            get => currentPage;
            set
            {
                if(value >= 1)
                {
                    currentPage = value;
                }
            }
        }

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

    }
}
