using System.Runtime.Serialization;

namespace Pixel.Automation.RestApi.Shared;

[DataContract]
[Serializable]
public enum ExpectedResponse
{
    Json,
    Xml,
    Text,
    File,
    Stream,
    Custom
}
