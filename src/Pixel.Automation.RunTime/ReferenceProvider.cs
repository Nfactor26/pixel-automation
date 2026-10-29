using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Automation.RunTime;

public class ReferenceProvider : IReferenceProvider
{
    private readonly ProjectReferences projectReferences;

    /// <summary>
    /// constructor
    /// </summary>  
    public ReferenceProvider(ProjectReferences projectReferences)
    {
        this.projectReferences = Guard.Argument(projectReferences, nameof(projectReferences)).NotNull();
    }

    ///<inheritdoc/> 
    public IEnumerable<ControlReference> GetControlReferences()
    {
        return projectReferences.ControlReferences;
    }

    ///<inheritdoc/> 
    public Version GetControlVersion(string applicationId, string controlId)
    {
        var controlReference = projectReferences.ControlReferences.FirstOrDefault(c => c.ApplicationId == applicationId && c.ControlId == controlId);
        if(controlReference is not null)
        {
            return controlReference.Version;
        }
        throw new ArgumentException($"No control with found with applicationId : '{applicationId}' and controlId : '{controlId}'");
    }

    ///<inheritdoc/> 
    public IEnumerable<PrefabReference> GetPrefabReferences()
    {
        return projectReferences.PrefabReferences;
    }

    ///<inheritdoc/> 
    public VersionInfo GetPrefabVersion(string applicationId, string prefabId)
    {
        var prefabReference = projectReferences.PrefabReferences.FirstOrDefault(p => p.ApplicationId == applicationId && p.PrefabId == prefabId);
        if (prefabReference is not null)
        {
            return prefabReference.Version;
        }
        throw new ArgumentException($"No prefab with found with applicationId : '{applicationId}' and prefabId : '{prefabId}'");
    }

    ///<inheritdoc/>  
    public IEnumerable<string> GetCodeEditorReferences()
    {
        return this.projectReferences.EditorReferences.GetCodeEditorReferences();
    }

    ///<inheritdoc/>  
    public IEnumerable<string> GetScriptEditorReferences()
    {
        return this.projectReferences.EditorReferences.GetScriptEditorReferences();
    }

    ///<inheritdoc/>  
    public IEnumerable<string> GetScriptEngineReferences()
    {
        return this.projectReferences.EditorReferences.ScriptEngineReferences;
    }

    ///<inheritdoc/>  
    public IEnumerable<string> GetImportsForScripts()
    {
        return this.projectReferences.EditorReferences.ScriptImports;
    }

    ///<inheritdoc/>  
    public IEnumerable<string> GetWhiteListedReferences()
    {
        return this.projectReferences.EditorReferences.WhiteListedReferences;
    }

}

public class ReferenceProviderFactory : IReferenceProviderFactory
{
    public IReferenceProvider CreateReferenceProvider(IFileSystem fileSystem)
    {
        var references = fileSystem.LoadFile<ProjectReferences>(fileSystem.ReferencesFile);
        return new ReferenceProvider(references);
    }
}
