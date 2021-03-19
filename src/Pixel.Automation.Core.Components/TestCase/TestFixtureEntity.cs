using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.TestCase
{
    /// <summary>
    /// TestFixtureEntity can have multiple test cases
    /// </summary>
    [DataContract]
    [Serializable]   
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
                this.AddComponent(new SetUpEntity());
                this.AddComponent(new TearDownEntity());
            }

        }
    }
}
