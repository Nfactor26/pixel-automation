using Pixel.Automation.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client.Interfaces;

public  interface IReferencesRepositoryClient
{
    /// <summary>
    /// Get the ProjectReferences for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <returns></returns>
    Task<ProjectReferences> GetProjectReferencesAsync(string projectId, string projectVersion);

    /// <summary>
    /// Add ProjectReferences for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="projectReferences"></param>
    /// <returns></returns>
    Task<ProjectReferences> AddProjectReferencesAsync(string projectId, string projectVersion, ProjectReferences projectReferences);

    /// <summary>
    /// Add or update a ControlReference for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="controlReference"></param>
    /// <returns></returns>
    Task AddOrUpdateControlReferences(string projectId, string projectVersion, ControlReference controlReference);
   
    /// <summary>
    /// Add or update a PrefabRerence for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="prefabReference"></param>
    /// <returns></returns>
    Task AddOrUpdatePrefabReferences(string projectId, string projectVersion, PrefabReference prefabReference);
     
    /// <summary>
    /// Set the EditorReferences for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="editorReferences"></param>
    /// <returns></returns>
    Task SetEditorReferencesAsync(string projectId, string projectVersion, EditorReferences editorReferences);

}
