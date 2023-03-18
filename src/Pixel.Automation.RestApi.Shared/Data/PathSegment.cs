using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.RestApi.Shared;

/// <summary>
/// PathSegment data for an <see cref="HttpRequest"/>
/// </summary>
[DataContract]
[Serializable]
public class PathSegment : NotifyPropertyChanged
{
    /// <summary>
    /// Indicates if PathSegment is enabled
    /// </summary>
    [DataMember]
    [DisplayName("Enabled")]
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Key for the path segment
    /// </summary>
    [DataMember]
    [DisplayName("Key")]
    public string SegmentKey { get; set; }
    
    /// <summary>
    /// Value of the path segment
    /// </summary>
    [DataMember]
    [DisplayName("Value")]  
    public Argument SegmentValue { get; set; } = new InArgument<string>() { DefaultValue = string.Empty };
       
    /// <summary>
    /// Description of the path segment
    /// </summary>
    [DataMember]
    public string Description { get; set; }
  
    /// <summary>
    /// constructor
    /// </summary>
    public PathSegment()
    {

    }
}
