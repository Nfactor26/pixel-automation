using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{
    [DataContract]
    [Serializable]
    public class TestStatistics : Document
    {     
        [DataMember]
        public string ProjectId { get; set; }

        [DataMember]
        public string ProjectName { get; set; }

        [DataMember]
        public string TestId { get; set; }

        [DataMember]
        public string TestName { get; set; }

        [DataMember]
        public string FixtureId { get; set; }

        [DataMember]
        public string FixtureName { get; set; }

        [DataMember]
        List<Label> Annotations { get; set; } = new List<Label>();

        /// <summary>
        /// Historical execution statistics on a monthly basis
        /// </summary>
        [DataMember]
        public List<TestExecutionStatistics> MonthlyStatistics { get; set; } = new List<TestExecutionStatistics>();
        
    }
        
}
