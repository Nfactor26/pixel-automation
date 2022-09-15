using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Scripting.Reference.Manager.Contracts;
using Pixel.Scripting.Reference.Manager.Models;

namespace Pixel.Scripting.Reference.Manager;

/// <summary>
/// Use ReferenceManagerFactor to create an instance of ReferenceManager for a given version of Automation Project or Prefab Project
/// </summary>
public class ReferenceManagerFactory : IReferenceManagerFactory
{
    private readonly ApplicationSettings applicationSettings;
    private readonly ISerializer serializer;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="applicationSettings"></param>
    /// <param name="serializer"></param>
    public ReferenceManagerFactory(ApplicationSettings applicationSettings, ISerializer serializer)
    {
        this.applicationSettings = Guard.Argument(applicationSettings).NotNull();
        this.serializer = Guard.Argument(serializer).NotNull().Value;
    }

    ///<inheritdoc/>  
    public ReferenceManager CreateForAutomationProject(AutomationProject project, VersionInfo version)
    {
        var referencesFiles = Path.Combine(applicationSettings.AutomationDirectory, project.ProjectId, version.ToString(), "AssemblyReferences.ref");
        if (!File.Exists(referencesFiles))
        {            
            serializer.Serialize<ReferenceCollection>(referencesFiles, CreateDefault());
        }
        var referenceCollection = serializer.Deserialize<ReferenceCollection>(referencesFiles);
        return new ReferenceManager(referenceCollection);
    }

    ///<inheritdoc/>  
    public ReferenceManager CreateForPrefabProject(PrefabProject project, VersionInfo version)
    {
        var referencesFiles = Path.Combine(applicationSettings.ApplicationDirectory, project.ApplicationId, Constants.PrefabsDirectory, project.PrefabId,
            version.ToString(), "AssemblyReferences.ref");
        if (!File.Exists(referencesFiles))
        {
            serializer.Serialize<ReferenceCollection>(referencesFiles, CreateDefault());
        }
        var referenceCollection = serializer.Deserialize<ReferenceCollection>(referencesFiles);
        return new ReferenceManager(referenceCollection);
    }

    /// <summary>
    /// Create a default  instance of RefereceCollection initialied with references data defined in application settings.
    /// </summary>
    /// <returns></returns>
    private ReferenceCollection CreateDefault()
    {
        var referenceCollection = new ReferenceCollection();
        foreach(var item in this.applicationSettings.DefaultEditorReferences ?? Enumerable.Empty<string>())
        {
           referenceCollection.CommonEditorReferences.Add(item);
        }
        foreach (var item in this.applicationSettings.DefaultCodeReferences ?? Enumerable.Empty<string>())
        {
            referenceCollection.CodeEditorReferences.Add(item);
        }
        foreach (var item in this.applicationSettings.DefaultScriptReferences ?? Enumerable.Empty<string>())
        {
            referenceCollection.ScriptEditorReferences.Add(item);
        }
        foreach (var item in this.applicationSettings.DefaultScriptReferences ?? Enumerable.Empty<string>())
        {
            referenceCollection.ScriptEngineReferences.Add(item);
        }
        foreach (var item in this.applicationSettings.DefaultScriptImports ?? Enumerable.Empty<string>())
        {
            referenceCollection.ScriptImports.Add(item);
        }
        foreach (var item in this.applicationSettings.WhiteListedReferences ?? Enumerable.Empty<string>())
        {
            referenceCollection.WhiteListedReferences.Add(item);
        }
        return referenceCollection;
    }
}
