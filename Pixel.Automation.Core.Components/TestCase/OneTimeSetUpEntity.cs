using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.TestCase
{

    [DataContract]
    [Serializable]
    [ToolBoxItem("One Time SetUp", "Test Entities", iconSource: null, description: "An Entity that contains a set of components that are  processed exacly once  before any of the test case is executed", tags: new string[] { "Test", "OneTimeSetUp" })]

    public class OneTimeSetUpEntity : Entity
    {
        public OneTimeSetUpEntity() : base("One Time SetUp", "OneTimeSetUp")
        {
        }
    }
}
