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
    /// Check if any version of control is in use by any version of any project
    /// </summary>
    /// <param name="controlId"></param>
    /// <returns></returns>
    Task<bool> IsControlInUse(string controlId);

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
    Task<bool> HasPrefabReference(string projectId, string projectVersion, PrefabReference prefabReference);

    /// <summary>
    /// Add PrefabReference for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="prefabReference"></param>
    /// <returns></returns>
    Task AddPrefabReference(string projectId, string projectVersion, PrefabReference prefabReference);

    /// <summary>
    /// Update an existing PrefabReference for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="prefabReference"></param>
    /// <returns></returns>
    Task UpdatePrefabReference(string projectId, string projectVersion, PrefabReference prefabReference);

    /// <summary>
    /// Add an entry for the fixtureId to the project references file. 
    /// </summary>
    /// <param name="projectId">Identifier of the project</param>
    /// <param name="projectVersion">Version of the project</param>
    /// <param name="fixtureId">Identifier of the fixture to add</param>
    /// <returns></returns>
    Task AddFixtureAsync(string projectId, string projectVersion, string fixtureId);

    /// <summary>
    /// Remove fixtureId from the project references file.
    /// </summary>
    /// <param name="projectId">Identifier of the project</param>
    /// <param name="projectVersion">Version of the project</param>
    /// <param name="fixtureId">Identifier of the fixture to remove</param>
    /// <returns></returns>
    Task DeleteFixtureAsync(string projectId, string projectVersion, string fixtureId);

    /// <summary>
    /// Add a new group for the test data source
    /// </summary>
    /// <param name="projectId">Identifier of the project</param>
    /// <param name="projectVersion">Version of the project</param>
    /// <param name="groupName">Name of the group to add</param>
    /// <returns></returns>
    Task AddDataSourceGroupAsync(string projectId, string projectVersion, string groupName);

    /// <summary>
    /// Rename an existing group for the test data source
    /// </summary>
    /// <param name="projectId">Identifier of the project</param>
    /// <param name="projectVersion">Version of the project</param>
    /// <param name="currentKey">Name of the existing group</param>
    /// <param name="newKey">New name of the group</param>
    /// <returns></returns>
    Task RenameDataSourceGroupAsync(string projectId, string projectVersion, string currentKey, string newKey);

    /// <summary>
    /// Add a test data source identifier to specified group
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="groupName"></param>
    /// <param name="testDataSourceId"></param>
    /// <returns></returns>
    Task AddDataSourceToGroupAsync(string projectId, string projectVersion, string groupName, string testDataSourceId);

    /// <summary>
    /// Move data source from one group to another
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="testDataSourceId"></param>
    /// <param name="currentGroup"></param>
    /// <param name="newGroup"></param>
    /// <returns></returns>
    Task MoveDataSourceToGroupAsync(string projectId, string projectVersion, string testDataSourceId, string currentGroup, string newGroup);

    /// <summary>
    /// Delete test data source identifier from specified group
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>    
    /// <param name="testDataSourceId"></param>
    /// <returns></returns>
    Task DeleteDataSourceAsync(string projectId, string projectVersion, string testDataSourceId);

}
