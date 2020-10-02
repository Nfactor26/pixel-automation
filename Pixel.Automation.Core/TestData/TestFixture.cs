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
        public string Description { get; set; }

        [DataMember]
        public string Group { get; set; } = "Default";

        [DataMember]
        public IEnumerable<string> Tags { get; set; } = new List<string>();

        public List<TestCase> Tests { get; set; } = new List<TestCase>();

        [DataMember]
        public bool IsMuted { get; set; }

        public Entity TestFixtureEntity { get; set; }

        [DataMember]
        public string ScriptFile { get; set; }

        public object Clone()
        {
            TestFixture copy = new TestFixture()
            {
                Id = this.Id,
                DisplayName = this.DisplayName,
                Description = this.Description,
                Tags = this.Tags,
                IsMuted = this.IsMuted
            };
            return copy;
        }

        public override string ToString()
        {
            return $"{Id}";
        }
    }
}
