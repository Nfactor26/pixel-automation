using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.TestCase
{

    [DataContract]
    [Serializable]
    [ToolBoxItem("TearDown", "Test Entities", iconSource: null, description: "An Entity that contains common set of components that are processed after each test case is executed", tags: new string[] { "Test", "TearDown" })]

    public class TearDownEntity : Entity
    {

        public TearDownEntity() : base("Tear Down", "TearDown")
        {
        }
     
    }
}
