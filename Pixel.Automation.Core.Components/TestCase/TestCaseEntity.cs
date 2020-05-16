using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.TestCase
{

    [DataContract]
    [Serializable]
    [ToolBoxItem("TestCase", "Test Entities", iconSource: null, description: "Test case entity contains all components that make up a test case", tags: new string[] { "Test" })]
    public class TestCaseEntity : Entity
    {

        public TestCaseEntity() : base("Test Case","TestCase")
        {
            
        }        

        public override void ResolveDependencies()
        {          
            if(this.components.Count==0)
            {
                this.AddComponent(new SetUpEntity());
                this.AddComponent(new TestSequenceEntity());
                this.AddComponent(new TearDownEntity());
            }  
           
        }
    }
}
