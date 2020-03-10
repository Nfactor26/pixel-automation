using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.TestData
{
    [DataContract]
    [Serializable]
    public class TestStatistics
    {
        [DataMember]
        public string TestCaseId { get; set; }

        [DataMember]
        public string CategoryId { get; set; }
        
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

        [DataMember]
        public int  LastExecutionTime { get; set; }    

    }


}
