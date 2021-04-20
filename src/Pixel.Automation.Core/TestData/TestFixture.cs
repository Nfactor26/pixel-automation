using Pixel.Automation.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.TestData
{
    [DataContract]
    [Serializable]
    [FileDescription("fixture")]
    public class TestFixture
    {
        [DataMember]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public bool IsMuted { get; set; }

        [DataMember]
        public string ScriptFile { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Category { get; set; } = "Default";

        /// <summary>
        /// Controls the delay for pre and post run of actors.
        /// </summary>
        [DataMember]
        public int DelayFactor { get; set; } = 3; 

        [DataMember]
        public TagCollection Tags { get; private set; } = new TagCollection();

        public List<TestCase> Tests { get; private set; } = new List<TestCase>();

        public Entity TestFixtureEntity { get; set; }   

        public object Clone()
        {
            TestFixture copy = new TestFixture()
            {               
                DisplayName = this.DisplayName,
                Order = this.Order,
                IsMuted = this.IsMuted,             
                Description = this.Description,
                Category = this.Category,
                DelayFactor = this.DelayFactor,
                Tags = this.Tags               
            };
            return copy;
        }

        public override string ToString()
        {
            return $"{Id}";
        }
    }
}
