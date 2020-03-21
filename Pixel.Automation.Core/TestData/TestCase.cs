using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.TestData
{

    [DataContract]
    [Serializable]
    public class TestCase : ICloneable
    {
        [DataMember]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [DataMember]
        public string CategoryId { get; set; } 
        
        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public IEnumerable<string> Tags { get; set; } = new List<string>();
     
        [DataMember]
        public bool IsMuted { get; set; }      

        [DataMember]
        public int Order { get; set; }
     
        public Entity TestCaseEntity { get; set; }

        [DataMember]
        public string ScriptFile { get; set; }
      
        [DataMember(IsRequired = false)]
        public string TestDataId { get; set; }
        

        public object Clone()
        {
            TestCase copy = new TestCase()
            {
                Id = this.Id,
                DisplayName = this.DisplayName,
                Description = this.Description,   
                Tags = this.Tags,
                IsMuted = this.IsMuted,               
                Order = this.Order,
                TestCaseEntity = this.TestCaseEntity                
            };
            return copy;
        }
    }

}
