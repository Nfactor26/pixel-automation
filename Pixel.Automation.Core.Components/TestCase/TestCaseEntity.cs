using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.TestCase
{
    /// <summary>
    /// Test case entity contains all components that make up a test case
    /// </summary>
    [DataContract]
    [Serializable]   
    public class TestCaseEntity : Entity
    {
        public TestCaseEntity() : base("Test Case","TestCase")
        {
            
        }  
    }
}
