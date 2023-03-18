using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.RestApi.Shared;

[DataContract]
[Serializable]
public enum AuthenticationMode
{
    None,
    Basic,
    OAuth1,
    OAuth2
}
