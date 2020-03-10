using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.TestCase
{

    [DataContract]
    [Serializable]
    [ToolBoxItem("SetUp", "Test Entities", iconSource: null, description: "An Entity that contains a set of components that are processed once before each test case is executed", tags: new string[] { "Test","SetUp" })]

    public class SetUpEntity : Entity
    {

        public SetUpEntity() : base("SetUp","SetUp")
        {
        }
    }
}
