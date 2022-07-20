using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Common;

[DataContract]
[Serializable]
public class ControlIdentifier
{
    /// <summary>
    /// Name of the attribute
    /// </summary>
    [DataMember]       
    public string AttributeName { get; set; }

    /// <summary>
    /// Value of the attribute
    /// </summary>
    [DataMember]
    public string AttributeValue { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public ControlIdentifier()
    {

    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="attributeName">Name of the attribute</param>
    /// <param name="attributeValue">Value of the attribute</param>
    public ControlIdentifier(string attributeName, string attributeValue)
    {
        this.AttributeName = attributeName;
        this.AttributeValue = attributeValue;
    }
}
