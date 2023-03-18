using Pixel.Automation.Core.Arguments;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.RestApi.Shared;

/// <summary>
/// Configuration data for executing an http request
/// </summary>
[DataContract]
[Serializable]
public class HttpRequest
{
    /// <summary>
    /// Type of request e.g. get, put, post, etc.
    /// </summary>
    [DataMember]
    public HttpAction RequestType { get; set; } = HttpAction.GET;
    
    /// <summary>
    /// Api endpoint
    /// </summary>
    [DataMember]
    [DisplayName("Target Url")]    
    public Argument TargetUrl { get; set; } = new InArgument<string>() { DefaultValue = string.Empty };

    /// <summary>
    /// Collection of <see cref="PathSegment"/> configured for http request
    /// </summary>
    [DataMember]
    [DisplayName("Path Segments")]
    public List<PathSegment> PathSegments { get; set; } = new();

    /// <summary>
    /// Collection of <see cref="QueryParameter"/> configured for http request
    /// </summary>
    [DataMember]
    [DisplayName("Request Parameters")]
    public List<QueryParameter> RequestParameters { get; set; } = new();

    /// <summary>
    /// Collection of <see cref="RequestHeader"/> configured for http request
    /// </summary>
    [DataMember]
    public List<RequestHeader> RequestHeaders { get; set; } = new();

    /// <summary>
    /// <see cref="BodyContent"/> configured for http request
    /// </summary>
    [DataMember]
    public BodyContent RequestBody { get; set; } = new FormDataBodyContent();
   
    /// <summary>
    /// constructor
    /// </summary>
    public HttpRequest()
    {

    }
}
