using System;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{ 
    [DataContract]
    [Serializable]
    public class TestStatistics
    {
        [DataMember]
        public string TestId { get; set; }

        [DataMember]
        public string TestName { get; set; }

        [DataMember]
        public string CategoryId { get; set; }

        [DataMember]
        public string CategoryName { get; set; }

        [DataMember]
        public int NumberOfTimesExeucted { get; set; }

        [DataMember]
        public int NumberOfTimesFailed { get; set; }

        [DataMember]
        public int NumberOfTimesPassed { get; set; }

        [DataMember]
        public double SuccessRate { get; set; }

        [DataMember]
        public int AverageExecutionTime { get; set; }

    }


}
