using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Controls
{
    [DataContract]
    [Serializable]
    public class ControlIdentifier
    {
        [DataMember]       
        public string AttributeName { get; set; }

        [DataMember]
        public string AttributeValue { get; set; }

        public ControlIdentifier()
        {

        }

        public ControlIdentifier(string attributeName, string attributeValue)
        {
            this.AttributeName = attributeName;
            this.AttributeValue = attributeValue;
        }
    }
}
