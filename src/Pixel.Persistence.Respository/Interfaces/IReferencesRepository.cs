using Pixel.Persistence.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository.Interfaces;

public interface IReferencesRepository
{
    /// <summary>
    /// Get the ProjectReferences for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <returns></returns>
    Task<ProjectReferences> GetProjectReferences(string projectId, string projectVersion);

    /// <summary>
    /// Add ProjectReferences for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="projectReferences"></param>
    /// <returns></returns>
    Task AddProjectReferences(string projectId, string projectVersion, ProjectReferences projectReferences);

    /// <summary>
    /// Set the EditorReferences for a given version of Project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="editorReferences"></param>
    /// <returns></returns>
    Task SetEditorReferences(string projectId, string projectVersion, EditorReferences editorReferences);


    /// <summary>
    /// Check if a given version of project has an existing reference to specified control
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="controlReference"></param>
    /// <returns></returns>
    Task<bool> HasControlReference(string projectId, string projectVersion, ControlReference controlReference);

    /// <summary>
    /// Add a new ControlReference to a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="controlReference"></param>
    /// <returns></returns>
    Task AddControlReference(string projectId, string projectVersion, ControlReference controlReference);

    /// <summary>
    /// Update existing ControlReference for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="controlReference"></param>
    /// <returns></returns>
    Task UpdateControlReference(string projectId, string projectVersion, ControlReference controlReference);


    /// <summary>
    /// Check if any version of prefab is in use by any version of any project
    /// </summary>
    /// <param name="prefabId"></param>
    /// <returns></returns>
    Task<bool> IsPrefabInUse(string prefabId);

    /// <summary>
    /// Check if any of the projets has a reference to any version of this prefab
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="controlReference"></param>
    /// <returns></returns>
    //Task<bool> HasPrefabReference(string projectId, string projectVersion, PrefabReference prefabReference);

    /// <summary>
    /// Add or update PrefabReferences for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="prefabReference"></param>
    /// <returns></returns>
    Task AddOrUpdatePrefabReference(string projectId, string projectVersion, PrefabReference prefabReference);

}
