using Pixel.Automation.Core.Interfaces;

namespace Pixel.Scripting.Reference.Manager.Contracts;

/// <summary>
/// IReferenceManagerFactory defined the contract for creating <see cref="ReferenceManager"/> for a given version of AutomationProject or PrefabProject
/// </summary>
public interface IReferenceManagerFactory
{
    /// <summary>
    /// Create a reference manager and initilize it for a given version of project to manage
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="fileSystem"></param>
    /// <returns></returns>
    IReferenceManager CreateReferenceManager(string projectId, string projectVersion, IFileSystem fileSystem);
}