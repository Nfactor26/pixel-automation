using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Application
{
    [DataContract]
    [Serializable]
    public class Application
    {
        [DataMember(IsRequired = true)]
        public string ApplicationId { get; set; }

        [DataMember(IsRequired = true)]
        public string DisplayName { get; set; }

        public Application()
        {
            this.ApplicationId = Guid.NewGuid().ToString();
        }
    }
}
