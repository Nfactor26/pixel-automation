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
    /// Add or update ControlReference for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="controlReference"></param>
    /// <returns></returns>
    Task AddOrUpdateControlReference(string projectId, string projectVersion, ControlReference controlReference);
   
    /// <summary>
    /// Add or update PrefabReferences for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="prefabReference"></param>
    /// <returns></returns>
    Task AddOrUpdatePrefabReference(string projectId, string projectVersion, PrefabReference prefabReference);

}
