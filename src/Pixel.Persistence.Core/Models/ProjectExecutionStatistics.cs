using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

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

        public double SuccessRate
        {
            get => (NumberOfTestsPassed / NumberOfTestsExecuted) * 100;
        }

        public ProjectExecutionStatistics(DateTime fromTime, DateTime toTime)
        {
            this.FromTime = fromTime;
            this.ToTime = toTime;
        }
    }
}
