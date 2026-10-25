using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Linq;

namespace Pixel.Automation.RunTime;

public class ReferenceProvider : IReferenceProvider
{
    private readonly ProjectReferences projectReferences;

    public ReferenceProvider(ProjectReferences projectReferences)
    {
        this.projectReferences = Guard.Argument(projectReferences, nameof(projectReferences)).NotNull();
    }

    public Version GetControlVersion(string applicationId, string controlId)
    {
        var controlReference = projectReferences.ControlReferences.FirstOrDefault(c => c.ApplicationId == applicationId && c.ControlId == controlId);
        if(controlReference is not null)
        {
            return controlReference.Version;
        }
        throw new ArgumentException($"No control with found with applicationId : '{applicationId}' and controlId : '{controlId}'");
    }

    public Version GetPrefabVersion(string applicationId, string prefabId)
    {
        var prefabReference = projectReferences.PrefabReferences.FirstOrDefault(p => p.ApplicationId == applicationId && p.PrefabId == prefabId);
        if (prefabReference is not null)
        {
            return prefabReference.Version;
        }
        throw new ArgumentException($"No prefab with found with applicationId : '{applicationId}' and prefabId : '{prefabId}'");
    }
}

public class ReferenceProviderFactory : IReferenceProviderFactory
{
    public IReferenceProvider CreateReferenceProvider(IFileSystem fileSystem)
    {
        var references = fileSystem.LoadFile<ProjectReferences>(fileSystem.ReferencesFile);
        return new ReferenceProvider(references);
    }
}
