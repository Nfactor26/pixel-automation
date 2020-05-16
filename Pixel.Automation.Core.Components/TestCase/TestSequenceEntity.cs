using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.TestCase
{

    [DataContract]
    [Serializable]
    [ToolBoxItem("Test Sequence", "Test Entities", iconSource: null, description: "Test Sequence is added as a child component for  a Test Case entity and acts as a place holder" +
        "for components that make up a test case", tags: new string[] { "Test" })]

    public class TestSequenceEntity : Entity
    {      
        public TestSequenceEntity() : base("Test Sequence", "TestSequence")
        {
        }
    }
}
