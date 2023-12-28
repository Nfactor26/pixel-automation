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


    /// <summary>
    /// Add a new group for test data source
    /// </summary>
    /// <param name="groupKey">name of the group</param>
    /// <returns></returns>
    Task AddTestDataSourceGroupAsync(string projectId, string projectVersion, string groupKey);

    /// <summary>
    /// Rename a test data source group
    /// </summary>
    /// <param name="currentKey">current group name</param>
    /// <param name="newKey">new group name</param>
    /// <returns></returns>
    Task RenameTestDataSourceGroupAsync(string projectId, string projectVersion, string currentKey, string newKey);

    /// <summary>
    /// Move a test data souce to a new group
    /// </summary>
    /// <param name="testDataSourceId">Identifier of the test data source</param>
    /// <param name="currentGroup">Name of current group</param>
    /// <param name="newGroup">Name of new group</param>
    /// <returns></returns>
    Task MoveTestDataSourceToGroupAsync(string projectId, string projectVersion, string testDataSourceId, string currentGroup, string newGroup);

}
