using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Reference.Manager.Contracts;

namespace Pixel.Scripting.Reference.Manager;

/// <summary>
/// Use ReferenceManagerFactor to create an instance of ReferenceManager for a given version of Automation Project or Prefab Project
/// </summary>
public class ReferenceManagerFactory : IReferenceManagerFactory
{
    private readonly ApplicationSettings applicationSettings;
    private readonly IReferencesRepositoryClient referencesRepositoryClient;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="applicationSettings"></param>
    /// <param name="serializer"></param>
    public ReferenceManagerFactory(ApplicationSettings applicationSettings, IReferencesRepositoryClient referencesRepositoryClient)
    {
        this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
        this.referencesRepositoryClient = Guard.Argument(referencesRepositoryClient, nameof(referencesRepositoryClient)).NotNull().Value;
    }

    public IReferenceManager CreateReferenceManager(string projectId, string projectVersion, IFileSystem fileSystem)
    {
        var referenceManager = new ReferenceManager(this.applicationSettings, this.referencesRepositoryClient);
        referenceManager.Initialize(projectId, projectVersion, fileSystem);
        return referenceManager;
    }
}
