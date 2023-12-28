using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Reference.Manager.Contracts;

/// <summary>
/// Manage resources owned or used by an <see cref="IProject"/> e.g. fixtures and test data sources belonging to project in addition to assmbly references and imports for code editor, script editor and script engine and controls
/// used in the project
/// </summary>
public interface IReferenceManager
{
    /// <summary>
    /// Initialize ReferenceManager for a given version of Project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="fileSystem"></param>
    void Initialize(string projectId, string projectVersion, IFileSystem fileSystem);

    /// <summary>
    /// Get the references for the data model project and associated code editor.
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> GetCodeEditorReferences();

    /// <summary>
    /// Get the imports that should be added to the script editors and script engines
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> GetImportsForScripts();

    /// <summary>
    /// Get the references for the script editor at design time
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> GetScriptEditorReferences();

    /// <summary>
    /// Get the references for script engine used at runtime
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> GetScriptEngineReferences();

    /// <summary>
    /// Get the white listed references allowed to be resolved by the script engine's CachedScriptMetadataResolver.
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> GetWhiteListedReferences();

    /// <summary>
    /// Get the <see cref="EditorReferences"/> for the project
    /// </summary>
    /// <returns></returns>
    EditorReferences GetEditorReferences();
  
    /// <summary>
    /// Get the <see cref="ControlReferences"/> for the project
    /// </summary>
    /// <returns></returns>
    ControlReferences GetControlReferences();
   
    /// <summary>
    /// Set the <see cref="ProjectReferences"/> for the ReferenceManager.
    /// </summary>
    /// <param name="projectReferences"></param>
    void SetProjectReferences(ProjectReferences projectReferences);

    /// <summary>
    /// Set the EditorReferences for the version of project being managed
    /// </summary>
    /// <param name="editorReferences"></param>
    /// <returns></returns>
    Task SetEditorReferencesAsync(EditorReferences editorReferences);

    /// <summary>
    /// Add a new ContolReferences to the version of project being managed
    /// </summary>
    /// <param name="controlReference"></param>
    /// <returns></returns>
    Task AddControlReferenceAsync(ControlReference controlReference);

    /// <summary>
    /// Update an existing ControlReference belonging to the version of project being managed e.g. version of control
    /// </summary>
    /// <param name="controlReference"></param>
    /// <returns></returns>
    Task UpdateControlReferenceAsync(ControlReference controlReference);

    /// <summary>
    /// Update multiple ControlReference belonging to the version of project being managed
    /// </summary>
    /// <param name="controlReferences"></param>
    /// <returns></returns>
    Task UpdateControlReferencesAsync(IEnumerable<ControlReference> controlReferences);  

    /// <summary>
    /// Get identifiers of all the fixtures owned by the project
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> GetAllFixtures();

    /// <summary>
    /// Add an entry for identifier of the fixture to the project (locally)
    /// </summary>
    /// <param name="fixtureId">Identifier of the fixture to be added</param>
    /// <returns></returns>
    void AddFixture(string fixtureId);

    /// <summary>
    /// Remove identifier entry of the fixture from the project (locally)
    /// </summary>
    /// <param name="fixtureId">Identifier of the fixture to be deleted</param>
    /// <returns></returns>
    void DeleteFixture(string fixtureId);

    /// <summary>
    /// Get identifiers of all the test data source that belong to a group given it's group key
    /// </summary>
    /// <param name="groupKey">Group to which the test data source belongs</param>
    /// <returns></returns>
    IEnumerable<string> GetTestDataSources(string groupKey);

    /// <summary>
    /// Add an entry for identifer of the test data source to the project (locally)
    /// </summary>
    /// <param name="groupKey">Group to which the test data source belongs</param>
    /// <param name="testDataSourceId">Identifier of the test data source</param>
    /// <returns></returns>
    void AddTestDataSource(string groupKey, string testDataSourceId);

    /// <summary>
    /// Remove test data source identifier from the specific group  (locally)
    /// </summary>
    /// <param name="groupKey"></param>
    /// <param name="testDataSourceId"></param>
    /// <returns></returns>
    void DeleteTestDataSource(string groupKey, string testDataSourceId);

    /// <summary>
    /// Get all the available test data source groups 
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> GetTestDataSourceGroups();

    /// <summary>
    /// Add a new group for test data source
    /// </summary>
    /// <param name="groupKey">name of the group</param>
    /// <returns></returns>
    Task AddTestDataSourceGroupAsync(string groupKey);

    /// <summary>
    /// Rename a test data source group
    /// </summary>
    /// <param name="currentKey">current group name</param>
    /// <param name="newKey">new group name</param>
    /// <returns></returns>
    Task RenameTestDataSourceGroupAsync(string currentKey, string newKey);

    /// <summary>
    /// Move a test data souce to a new group
    /// </summary>
    /// <param name="testDataSourceId">Identifier of the test data source</param>
    /// <param name="currentGroup">Name of current group</param>
    /// <param name="newGroup">Name of new group</param>
    /// <returns></returns>
    Task MoveTestDataSourceToGroupAsync(string testDataSourceId, string currentGroup, string newGroup);

    /// <summary>
    /// Get the <see cref="PrefabReferences"/> for the project
    /// </summary>
    /// <returns></returns>
    PrefabReferences GetPrefabReferences();

    /// <summary>
    /// Add a new PrefabReference to the version of project being managed
    /// </summary>
    /// <param name="prefabReference"></param>
    /// <returns></returns>
    Task AddPrefabReferenceAsync(PrefabReference prefabReference);

    /// <summary>
    /// Update an existing PrefabReferences belonging to the version of project being managed e.g. version of prefab
    /// </summary>
    /// <param name="prefabReference"></param>
    /// <returns></returns>
    Task UpdatePrefabReferenceAsync(PrefabReference prefabReference);

    /// <summary>
    /// Update multiple ContolReference belonging to the version of project being managed
    /// </summary>
    /// <param name="prefabReferences"></param>
    /// <returns></returns>
    Task UpdatePrefabReferencesAsync(IEnumerable<PrefabReference> prefabReferences);

}
