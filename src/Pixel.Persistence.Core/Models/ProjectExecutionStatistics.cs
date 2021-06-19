using System;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{
    [DataContract]
    public class ProjectExecutionStatistics
    {
        [DataMember]
        public DateTime FromTime { get; private set; }

        [DataMember]
        public DateTime ToTime { get; private set; }

        [DataMember]
        public int NumberOfTestsExecuted { get; set; }

        [DataMember]
        public int NumberOfTestsFailed { get; set; }

        [DataMember]
        public int NumberOfTestsPassed { get; set; }    
        
        /// <summary>
        /// Total execution time for successful tests in seconds
        /// </summary>
        [DataMember]
        public double TotalExecutionTime { get; set; }

        [IgnoreDataMember]
        public double AvgExecutionTime
        {
            get
            {
                if (NumberOfTestsPassed > 0)
                {
                    return TotalExecutionTime / NumberOfTestsPassed;
                }
                return 0;
            }
        }

        [IgnoreDataMember]
        public double SuccessRate
        {
            get
            {
                if (NumberOfTestsExecuted > 0)
                {
                    return (NumberOfTestsPassed / NumberOfTestsExecuted) * 100;
                }
                return 0;
            }
        }

        public ProjectExecutionStatistics(DateTime fromTime, DateTime toTime)
        {
            this.FromTime = fromTime;
            this.ToTime = toTime;
        }
    }
}
