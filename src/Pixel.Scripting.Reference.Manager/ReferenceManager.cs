using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Scripting.Reference.Manager.Contracts;
using Pixel.Persistence.Services.Client.Interfaces;

namespace Pixel.Scripting.Reference.Manager;

/// <summary>
/// ReferenceManager is used to retrieve required assembly references and imports for code editor , script editor and script runtime for a automation project 
/// or prefab project
/// </summary>
internal class ReferenceManager : IReferenceManager
{
    private readonly ApplicationSettings applicationSettings;
    private readonly IReferencesRepositoryClient referencesRepositoryClient;
    private string projectId;
    private string projectVersion;
    private IFileSystem fileSystem;
    private ProjectReferences projectReferences;
    private ControlReferences controlReferences;
    private PrefabReferences prefabReferences;
  
    bool IsOnlineMode => !this.applicationSettings.IsOfflineMode;

    /// <summary>
    /// constructor
    /// </summary>   
    /// <param name="projectReferences"></param>
    public ReferenceManager(ApplicationSettings applicationSettings, IReferencesRepositoryClient referencesRepositoryClient)
    {
        this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
        this.referencesRepositoryClient = Guard.Argument(referencesRepositoryClient, nameof(referencesRepositoryClient)).NotNull().Value;
    }

    /// <inheritdoc/>
    public void Initialize(string projectId, string projectVersion, IFileSystem fileSystem)
    {
        this.projectId = Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        this.projectVersion = Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        this.fileSystem = Guard.Argument(fileSystem, nameof(fileSystem)).NotNull().Value;      
        if(File.Exists(this.fileSystem.ReferencesFile))
        {
            this.projectReferences = this.fileSystem.LoadFile<ProjectReferences>(this.fileSystem.ReferencesFile);
        }
        else
        {
            this.projectReferences = CreateDefault();
            _ = this.referencesRepositoryClient.AddProjectReferencesAsync(this.projectId, this.projectVersion, this.projectReferences);
            SaveLocal();
        }

        this.controlReferences = new ControlReferences(this.projectReferences.ControlReferences);
        this.prefabReferences = new PrefabReferences(this.projectReferences.PrefabReferences);
    }

    ///<inheritdoc/>  
    public EditorReferences GetEditorReferences()
    {
        return this.projectReferences.EditorReferences;
    }

    ///<inheritdoc/>  
    public ControlReferences GetControlReferences()
    {
        return this.controlReferences;
    }

    ///<inheritdoc/>  
    public PrefabReferences GetPrefabReferences()
    {
        return this.prefabReferences;
    }

    ///<inheritdoc/>  
    public IEnumerable<string> GetCodeEditorReferences()
    {
        return this.projectReferences.EditorReferences.CommonEditorReferences.Union(this.projectReferences.EditorReferences.CodeEditorReferences);
    }

    ///<inheritdoc/>  
    public IEnumerable<string> GetScriptEditorReferences()
    {
        return this.projectReferences.EditorReferences.CommonEditorReferences.Union(this.projectReferences.EditorReferences.ScriptEditorReferences);
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

    ///<inheritdoc/>  
    public void SetProjectReferences(ProjectReferences projectReferences)
    {
        this.projectReferences = Guard.Argument(projectReferences).NotNull();
    }

    /// <inheritdoc/>
    public async Task SetEditorReferencesAsync(EditorReferences editorReferences)
    {
        Guard.Argument(editorReferences, nameof(editorReferences)).NotNull();
        this.projectReferences.EditorReferences = editorReferences;
        if (IsOnlineMode)
        {
            await this.referencesRepositoryClient.SetEditorReferencesAsync(this.projectId, this.projectVersion, editorReferences);
        }
        SaveLocal();
    }

    /// <inheritdoc/>
    public async Task AddControlReferenceAsync(ControlReference controlReference)
    {
        Guard.Argument(controlReference, nameof(controlReference)).NotNull();
        if (!this.controlReferences.HasReference(controlReference))
        {
            this.controlReferences.AddControlReference(controlReference);
            if (IsOnlineMode)
            {
                await this.referencesRepositoryClient.AddOrUpdateControlReferences(this.projectId, this.projectVersion, controlReference);
            }
            SaveLocal();
        }
    }

    /// <inheritdoc/>
    public async Task UpdateControlReferenceAsync(ControlReference controlReference)
    {
        Guard.Argument(controlReference, nameof(controlReference)).NotNull();
        if (this.controlReferences.HasReference(controlReference))
        {
            var existingReference = this.controlReferences.GetControlReference(controlReference.ControlId);
            existingReference.Version = controlReference.Version;
            if (IsOnlineMode)
            {
                await this.referencesRepositoryClient.AddOrUpdateControlReferences(this.projectId, this.projectVersion, controlReference);
            }
            SaveLocal();
        }
    }

    /// <inheritdoc/>
    public async Task UpdateControlReferencesAsync(IEnumerable<ControlReference> controlReferences)
    {
        Guard.Argument(controlReferences, nameof(controlReferences)).NotNull();
        foreach(var controlReference in controlReferences)
        {
            if (this.controlReferences.HasReference(controlReference))
            {
                var existingReference = this.controlReferences.GetControlReference(controlReference.ControlId);
                existingReference.Version = controlReference.Version;
                if (IsOnlineMode)
                {
                    await this.referencesRepositoryClient.AddOrUpdateControlReferences(this.projectId, this.projectVersion, controlReference);
                }
                SaveLocal();
            }
        }      
    }

    /// <inheritdoc/>
    public async Task AddPrefabReferenceAsync(PrefabReference prefabReference)
    {
        Guard.Argument(prefabReference, nameof(prefabReference)).NotNull();
        if (!this.prefabReferences.HasReference(prefabReference))
        {
            this.prefabReferences.AddPrefabReference(prefabReference);
            if (IsOnlineMode)
            {
                await this.referencesRepositoryClient.AddOrUpdatePrefabReferences(this.projectId, this.projectVersion, prefabReference);
            }
            SaveLocal();
        }
    }

    /// <inheritdoc/>
    public async Task UpdatePrefabReferenceAsync(PrefabReference prefabReference)
    {
        Guard.Argument(prefabReference, nameof(prefabReference)).NotNull();
        if (this.prefabReferences.HasReference(prefabReference))
        {
            var existingReference = this.prefabReferences.GetPrefabReference(prefabReference.PrefabId);
            existingReference.Version = prefabReference.Version;
            if (IsOnlineMode)
            {
                await this.referencesRepositoryClient.AddOrUpdatePrefabReferences(this.projectId, this.projectVersion, prefabReference);
            }
            SaveLocal();
        }
    }

    /// <inheritdoc/>
    public async Task UpdatePrefabReferencesAsync(IEnumerable<PrefabReference> prefabReferences)
    {
        Guard.Argument(prefabReferences, nameof(prefabReferences)).NotNull();
        foreach (var prefabReference in prefabReferences)
        {
            if (this.prefabReferences.HasReference(prefabReference))
            {
                var existingReference = this.prefabReferences.GetPrefabReference(prefabReference.PrefabId);
                existingReference.Version = prefabReference.Version;
                if (IsOnlineMode)
                {
                    await this.referencesRepositoryClient.AddOrUpdatePrefabReferences(this.projectId, this.projectVersion, prefabReference);
                }
                SaveLocal();
            }
        }
    }

    private void SaveLocal()
    {
        this.fileSystem.SaveToFile<ProjectReferences>(projectReferences, Path.GetDirectoryName(this.fileSystem.ReferencesFile), Path.GetFileName(this.fileSystem.ReferencesFile));       
    }
    
    private ProjectReferences CreateDefault()
    {
        var projectReferences = new ProjectReferences();
        foreach (var item in this.applicationSettings.DefaultEditorReferences ?? Enumerable.Empty<string>())
        {
            projectReferences.EditorReferences.CommonEditorReferences.Add(item);
        }
        foreach (var item in this.applicationSettings.DefaultCodeReferences ?? Enumerable.Empty<string>())
        {
            projectReferences.EditorReferences.CodeEditorReferences.Add(item);
        }
        foreach (var item in this.applicationSettings.DefaultScriptReferences ?? Enumerable.Empty<string>())
        {
            projectReferences.EditorReferences.ScriptEditorReferences.Add(item);
        }
        foreach (var item in this.applicationSettings.DefaultScriptReferences ?? Enumerable.Empty<string>())
        {
            projectReferences.EditorReferences.ScriptEngineReferences.Add(item);
        }
        foreach (var item in this.applicationSettings.DefaultScriptImports ?? Enumerable.Empty<string>())
        {
            projectReferences.EditorReferences.ScriptImports.Add(item);
        }
        foreach (var item in this.applicationSettings.WhiteListedReferences ?? Enumerable.Empty<string>())
        {
            projectReferences.EditorReferences.WhiteListedReferences.Add(item);
        }
        return projectReferences;
    }
}
