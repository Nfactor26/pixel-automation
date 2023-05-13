using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using RestSharp;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.RestApi.Components;

[DataContract]
[Serializable]
[DisplayName("Rest Api")]
[Description("Rest Api")]
[ApplicationEntity(typeof(RestApiApplicationEntity))]
[SupportedPlatforms("WINDOWS", "LINUX", "OSX")]
public class RestApiApplication : Application
{
    /// <summary>
    /// Base url for the rest api endpoint
    /// </summary>
    [DataMember]
    [Display(Name = "Base Url", Order = 30, Description = "Base url of the rest api endpoint")]
    public string BaseUrl { get; set; }  

    /// <summary>
    /// RestClient used to execute http requests
    /// </summary>
    [IgnoreDataMember]
    [Browsable(false)]
    public RestClient RestClient { get; internal set; }
}
