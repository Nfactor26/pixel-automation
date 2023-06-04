using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{
    [DataContract]
    public class ProjectStatistics : Document
    {  
        [DataMember]
        public string ProjectId { get; set; }

        [DataMember]
        public string ProjectName { get; set; }

        /// <summary>
        /// Historical execution statistics on a monthly basis
        /// </summary>
        [DataMember]
        public List<ProjectExecutionStatistics> MonthlyStatistics { get; set; } = new List<ProjectExecutionStatistics>();     
      
    }
}
