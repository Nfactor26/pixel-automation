using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using System.Runtime.Serialization;

namespace Pixel.Automation.RestApi.Shared;

/// <summary>
/// QueryParameter data for <see cref="HttpRequest"/>
/// </summary>
[DataContract]
[Serializable]
public class QueryParameter : NotifyPropertyChanged
{
    /// <summary>
    /// Indicates if QueryParameter is enabled
    /// </summary>
    [DataMember]
    public bool IsEnabled { get; set; } = true;
      
    /// <summary>
    /// Key for the Query Parameter
    /// </summary>
    [DataMember]  
    public string QueryStringKey { get; set; }
  
    /// <summary>
    /// Value of the QueryParameter
    /// </summary>
    [DataMember] 
    public Argument QueryStringValue { get; set; } = new InArgument<string>() { DefaultValue = string.Empty };
 
    /// <summary>
    /// Description for the QueryParameter
    /// </summary>
    [DataMember]
    public string Description { get; set; }
   
    /// <summary>
    /// constructor
    /// </summary>
    public QueryParameter()
    {

    }

}
