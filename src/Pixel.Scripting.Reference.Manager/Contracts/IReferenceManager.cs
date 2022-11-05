using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;

namespace Pixel.Scripting.Reference.Manager.Contracts;

/// <summary>
/// IReferenceManager defines the contract for retrieving assmbly references and imports for code editor, script editor and script engine associated
/// with automation or prefab projects
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
    /// Get the <see cref="PrefabReferences"/> for the project
    /// </summary>
    /// <returns></returns>
    PrefabReferences GetPrefabReferences();

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