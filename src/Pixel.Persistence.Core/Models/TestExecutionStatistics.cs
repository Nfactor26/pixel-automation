using System;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{
    [DataContract]
    public class TestExecutionStatistics
    {
        [DataMember]
        public string ProjectVersion { get; set; }

        [DataMember]
        public DateTime FromTime { get; private set; }

        [DataMember]
        public DateTime ToTime { get; private set; }

        [DataMember]
        public int NumberOfTimesExecuted { get; set; }

        [DataMember]
        public int NumberOfTimesFailed { get; set; }

        [DataMember]
        public int NumberOfTimesPassed { get; set; }

        [DataMember]
        public double MinExecutionTime { get; set; }

        [DataMember]
        public double MaxExecutionTime { get; set; }

        /// <summary>
        /// Total execution time for the passed tests
        /// </summary>
        [DataMember]
        public double TotalExecutionTime { get; set; }

        [IgnoreDataMember]
        public double AvgExecutionTime
        {
            get
            {
                if(NumberOfTimesPassed > 0)
                {
                    return TotalExecutionTime / NumberOfTimesPassed;                    
                }
                return 0;
            }
        }

        [IgnoreDataMember]
        public double SuccessRate
        {
            get
            {
                if(NumberOfTimesExecuted > 0)
                {
                    return (NumberOfTimesPassed / NumberOfTimesExecuted) * 100;
                }
                return 0;
            }
        }

        public TestExecutionStatistics(string projectVersion, DateTime fromTime, DateTime toTime)
        {
            this.ProjectVersion = projectVersion;
            this.FromTime = fromTime;
            this.ToTime = toTime;
        }

    }
}
