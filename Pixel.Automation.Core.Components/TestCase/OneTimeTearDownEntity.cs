using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.TestCase
{

    [DataContract]
    [Serializable]
    [ToolBoxItem("One Time TearDown", "Test Entities", iconSource: null, description: "An Entity that contains a set of components that are  processed exacly once  after any of the test case is executed", tags: new string[] { "Test", "OneTimeTearDown" })]

    public class OneTimeTearDownEntity : Entity
    {
        public OneTimeTearDownEntity() : base("One Time TearDown","OneTimeTearDown")
        {
        }

    }
}
