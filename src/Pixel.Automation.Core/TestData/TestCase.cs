using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.TestData
{

    [DataContract]
    [Serializable]
    [FileDescription("test")]
    public class TestCase : ICloneable
    {
        [DataMember]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [DataMember]
        public string FixtureId { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public bool IsMuted { get; set; }

        [DataMember]
        public Priority Priority { get; set; }

        [DataMember]
        public string ScriptFile { get; set; }


        [DataMember(IsRequired = false)]
        public string TestDataId { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public TagCollection Tags { get;  private set; } = new TagCollection();     
     
     
        public Entity TestCaseEntity { get; set; }  
        

        public object Clone()
        {
            TestCase copy = new TestCase()
            {              
                DisplayName = this.DisplayName,
                Description = this.Description,   
                Tags = new TagCollection(this.Tags.Tags),
                IsMuted = this.IsMuted,               
                Order = this.Order            
            };
            return copy;
        }

        public override string ToString()
        {
            return $"{Id}";
        }
    }

}
