using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models;

[DataContract]
public class ApplicationScreen
{
    [DataMember(Order = 10)]
    public string ScreenId { get; set; }

    [DataMember(Order = 20)]
    public string ScreenName { get; set; }

    [DataMember(Order = 30)]
    public List<string> AvailableControls { get; private set; } = new();

    [DataMember(Order = 40)]
    public List<string> AvailablePrefabs { get; private set; } = new();

    public ApplicationScreen()
    {

    }
}
