using Pixel.Persistence.Core.Models;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Request;

[DataContract]
public class AddPrefabVersionRequest
{
    [DataMember(Order = 10)]
    public PrefabVersion NewVersion { get; set; }

    [DataMember(Order = 20)]
    public PrefabVersion CloneFrom { get; set; }

    public AddPrefabVersionRequest()
    {

    }

    public AddPrefabVersionRequest(PrefabVersion newVersion, PrefabVersion cloneFrom)
    {
        NewVersion = newVersion;
        CloneFrom = cloneFrom;
    }
}

