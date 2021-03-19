using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.TestCase
{
    /// <summary>
    /// An Entity that contains a set of components that are processed once before each test case is executed
    /// </summary>
    [DataContract]
    [Serializable]   
    public class SetUpEntity : Entity
    {

        public SetUpEntity() : base("SetUp", "SetUp")
        {
        }
    }
}
