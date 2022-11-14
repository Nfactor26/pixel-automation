using Pixel.Persistence.Core.Models;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Request;

[DataContract]
public class AddProjectVersionRequest
{
    [DataMember(Order = 10)]
    public ProjectVersion NewVersion { get; set; }

    [DataMember(Order = 20)]
    public ProjectVersion CloneFrom { get; set; }

    public AddProjectVersionRequest()
    {

    }

    public AddProjectVersionRequest(ProjectVersion newVersion, ProjectVersion cloneFrom)
    {
        NewVersion = newVersion;
        CloneFrom = cloneFrom;
    }
}

