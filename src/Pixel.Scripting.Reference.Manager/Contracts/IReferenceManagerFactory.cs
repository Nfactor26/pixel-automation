using Pixel.Automation.Core.Models;

namespace Pixel.Scripting.Reference.Manager.Contracts;

/// <summary>
/// IReferenceManagerFactory defined the contract for creating <see cref="ReferenceManager"/> for a given version of AutomationProject or PrefabProject
/// </summary>
public interface IReferenceManagerFactory
{
    /// <summary>
    /// Create <see cref="ReferenceManager"/> for the active version of a given AutomationProject
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    ReferenceManager CreateForAutomationProject(AutomationProject project, VersionInfo version);

    /// <summary>
    /// Create <see cref="ReferenceManager"/> for the active verison of a given PrefabProject
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    ReferenceManager CreateForPrefabProject(PrefabProject project, VersionInfo version);
}