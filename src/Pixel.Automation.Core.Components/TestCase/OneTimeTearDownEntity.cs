using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.TestCase;

/// <summary>
/// An Entity that contains a set of components that are  processed exacly once  after any of the test case is executed
/// </summary>
[DataContract]
[Serializable]   
public class OneTimeTearDownEntity : Entity
{
    public OneTimeTearDownEntity() : base("One Time TearDown", "OneTimeTearDown")
    {
    }

}
