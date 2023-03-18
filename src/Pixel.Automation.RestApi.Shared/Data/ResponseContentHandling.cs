using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using System.Runtime.Serialization;

namespace Pixel.Automation.RestApi.Shared;

/// <summary>
/// Captures the settings for handling the response received after executing request
/// </summary>
[DataContract]
[Serializable]    
public class ResponseContentHandling : NotifyPropertyChanged
{
    ExpectedResponse expectedResponseType = ExpectedResponse.Custom;
    [DataMember]
    public ExpectedResponse ExpectedResponseType
    {
        get => expectedResponseType;
        set
        {
            expectedResponseType = value;
            switch(expectedResponseType)
            {
                case ExpectedResponse.Json:
                case ExpectedResponse.Xml:
                    SaveTo = new OutArgument<object>() { CanChangeType = true, AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };
                    break;
                case ExpectedResponse.Text:
                    SaveTo = new OutArgument<string>() { CanChangeType = false, AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };
                    break;
                case ExpectedResponse.Stream:
                    SaveTo = new OutArgument<Stream>() { CanChangeType = false, AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };
                    break;
                case ExpectedResponse.File:
                    SaveTo = new InArgument<string>() { DefaultValue = string.Empty };
                    break;
                case ExpectedResponse.Custom:
                    SaveTo = new OutArgument<HttpResponse>() { CanChangeType = false, AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };
                    break;
            }
            OnPropertyChanged(nameof(ExpectedResponseType));
            OnPropertyChanged(nameof(SaveTo));
        }
    }


    Argument saveTo = new OutArgument<HttpResponse>() { CanChangeType = false, AllowedModes = ArgumentMode.DataBound|ArgumentMode.Scripted, Mode = ArgumentMode.DataBound};
    /// <summary>
    /// Target variable to which response should be saved.
    /// For json/xml as expected response, desirialized data will be assigned.
    /// For string as expected response, string data will be assigned.
    /// For stream as expected response, <see cref="Stream"/> will be assigned.
    /// For custom as expected response, <see cref="IRestResponse"/> will be assigned.
    /// Type of the InArgument will be automatically set to correct type for string/stream/custom expected responses.
    /// </summary>
    [DataMember]       
    public Argument SaveTo
    {
        get => this.saveTo;
        set
        {
            this.saveTo = value;
            OnPropertyChanged(nameof(SaveTo));
        }
    }  
}
