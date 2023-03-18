using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.RestApi.Shared;

[DataContract]
[Serializable]
public class RequestHeader : NotifyPropertyChanged
{
    /// <summary>
    /// Indicates if RequestHeader row is enabled
    /// </summary>
    [DataMember(Order = 10)]
    [DisplayName("Enabled")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Key for the request header
    /// </summary>
    [DataMember(Order = 20)]
    [DisplayName("Key")]
    public string HeaderKey { get; set; }
  
    /// <summary>
    /// Value of the Request header
    /// </summary>
    [DataMember]
    [DisplayName("Value")]   
    public Argument HeaderValue { get; set; } = new InArgument<string>() { DefaultValue = string.Empty, CanChangeType = false, Mode = ArgumentMode.Default };

    /// <summary>
    /// Description of the request header
    /// </summary>
    [DataMember]
    public string Description { get; set; }

    //constructor
    public RequestHeader()
    {

    }

}
