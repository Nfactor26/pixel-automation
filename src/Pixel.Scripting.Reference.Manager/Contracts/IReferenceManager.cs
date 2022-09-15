namespace Pixel.Scripting.Reference.Manager.Contracts;

/// <summary>
/// IReferenceManager defines the contract for retrieving assmbly references and imports for code editor, script editor and script engine associated
/// with automation or prefab projects
/// </summary>
public interface IReferenceManager
{
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
}