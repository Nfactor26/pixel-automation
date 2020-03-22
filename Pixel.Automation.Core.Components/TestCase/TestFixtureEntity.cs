using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.TestCase
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("TestFixture", "Test Entities", iconSource: null, description: "Test Fixture Entity", tags: new string[] { "Test" })]

    public class TestFixtureEntity : Entity
    {
       
        public TestFixtureEntity() : base("Test Fixture", "TestFixture")
        {

        }
     
        public override void ResolveDependencies()
        {
            if (this.components.Count == 0)
            {
                this.AddComponent(new OneTimeSetUpEntity());
                this.AddComponent(new OneTimeTearDownEntity());               
            }

        }
    }
}
