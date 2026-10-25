using System;

namespace Pixel.Automation.Core.Interfaces;

public interface IReferenceProvider
{   
    Version GetControlVersion(string applicationId, string controlId);

    Version GetPrefabVersion(string applicationId, string controlId);
}

public interface IReferenceProviderFactory
{
    IReferenceProvider CreateReferenceProvider(IFileSystem fileSystem);
}
