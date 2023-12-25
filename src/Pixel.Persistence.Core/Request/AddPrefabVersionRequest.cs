using Pixel.Persistence.Core.Models;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Request;

[DataContract]
public class AddPrefabVersionRequest
{
    [DataMember(Order = 10)]
    public VersionInfo NewVersion { get; set; }

    [DataMember(Order = 20)]
    public VersionInfo CloneFrom { get; set; }

    public AddPrefabVersionRequest()
    {

    }

    public AddPrefabVersionRequest(VersionInfo newVersion, VersionInfo cloneFrom)
    {
        NewVersion = newVersion;
        CloneFrom = cloneFrom;
    }
}

