using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces;

public interface IReferenceProvider
{
    /// <summary>
    /// Get the version of the control in use
    /// </summary> 
    Version GetControlVersion(string applicationId, string controlId);

    /// <summary>
    /// Get control referencces used in project
    /// </summary> 
    IEnumerable<ControlReference> GetControlReferences();

    /// <summary>
    /// Get the VersionInfo for the Prefab
    /// </summary>  
    VersionInfo GetPrefabVersion(string applicationId, string prefabId);

    /// <summary>
    /// Get Prefab references used in project
    /// </summary> 
    IEnumerable<PrefabReference> GetPrefabReferences();

    /// <summary>
    /// Get the references for the data model project and associated code editor.
    /// </summary> 
    IEnumerable<string> GetCodeEditorReferences();

    /// <summary>
    /// Get the imports that should be added to the script editors and script engines
    /// </summary>
    IEnumerable<string> GetImportsForScripts();

    /// <summary>
    /// Get the references for the script editor at design time
    /// </summary>    
    IEnumerable<string> GetScriptEditorReferences();

    /// <summary>
    /// Get the references for script engine used at runtime
    /// </summary>  
    IEnumerable<string> GetScriptEngineReferences();

    /// <summary>
    /// Get the white listed references allowed to be resolved by the script engine's CachedScriptMetadataResolver.
    /// </summary> 
    IEnumerable<string> GetWhiteListedReferences();
}

public interface IReferenceProviderFactory
{
    /// <summary>
    /// Create a <see cref="IReferenceProvider"/> given the <see cref="IFileSystem"/> of Project
    /// </summary>
    /// <param name="fileSystem">IFileSystem for the Project</param>
    /// <returns></returns>
    IReferenceProvider CreateReferenceProvider(IFileSystem fileSystem);
}
