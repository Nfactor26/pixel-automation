using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Controls;

[DataContract]
[Serializable]
public class ControlIdentifier
{
    /// <summary>
    /// Name of the attribute
    /// </summary>
    [DataMember(Order = 10)]
    public string AttributeName { get; set; }

    /// <summary>
    /// Value of the attribute
    /// </summary>
    [DataMember(Order = 20)]
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
