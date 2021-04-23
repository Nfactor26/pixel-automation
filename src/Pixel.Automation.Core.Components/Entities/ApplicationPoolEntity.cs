using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components
{
    [DataContract]
    [Serializable]   
    public class ApplicationPoolEntity : Entity
    {
        public ApplicationPoolEntity() : base("Application Pool", "ApplicationPoolEntity")
        {

        }
    }
}
