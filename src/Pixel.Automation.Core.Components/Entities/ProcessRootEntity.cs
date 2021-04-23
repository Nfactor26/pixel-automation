using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.Entities
{
    [DataContract]
    [Serializable]
    public class  ProcessRootEntity : Entity
    {
        public ProcessRootEntity() : base("Automation Process", "RootEntity")
        {

        }
    }
}
