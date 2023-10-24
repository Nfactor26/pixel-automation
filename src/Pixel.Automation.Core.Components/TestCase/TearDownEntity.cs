using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.TestCase;

/// <summary>
/// An Entity that contains common set of components that are processed after each test case is executed
/// </summary>
[DataContract]
[Serializable]    
public class TearDownEntity : Entity
{
    public TearDownEntity() : base("Tear Down", "TearDown")
    {
    }
 
}
