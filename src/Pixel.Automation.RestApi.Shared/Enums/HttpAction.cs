using System.Runtime.Serialization;

namespace Pixel.Automation.RestApi.Shared;

[DataContract]
[Serializable]
public enum HttpAction
{
    GET = 0,
    POST = 1,
    PUT = 2,
    DELETE = 3,
    HEAD = 4,
    OPTIONS = 5,
    PATCH = 6,
    MERGE = 7
}

