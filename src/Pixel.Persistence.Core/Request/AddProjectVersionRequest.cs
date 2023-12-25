using Pixel.Persistence.Core.Models;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Request;

[DataContract]
public class AddProjectVersionRequest
{
    [DataMember(Order = 10)]
    public VersionInfo NewVersion { get; set; }

    [DataMember(Order = 20)]
    public VersionInfo CloneFrom { get; set; }

    public AddProjectVersionRequest()
    {

    }

    public AddProjectVersionRequest(VersionInfo newVersion, VersionInfo cloneFrom)
    {
        NewVersion = newVersion;
        CloneFrom = cloneFrom;
    }
}

