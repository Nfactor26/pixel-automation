using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Persistence.Services.Client.Interfaces;

namespace Pixel.Automation.Reference.Manager;

/// <summary>
/// ReferenceManager is used to retrieve required assembly references and imports for code editor , script editor and script runtime for a automation project 
/// or prefab project
/// </summary>
internal class ReferenceManager : IReferenceManager
{
    private ProjectReferences projectReferences;
    private readonly ApplicationSettings applicationSettings;
    private readonly IReferencesRepositoryClient referencesRepositoryClient;
    private string projectId;
    private string projectVersion;
    private IFileSystem fileSystem;
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
            if(!applicationSettings.IsOfflineMode)
            {
                _ = this.referencesRepositoryClient.AddProjectReferencesAsync(this.projectId, this.projectVersion, this.projectReferences);
            }
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
            if (IsOnlineMode)
            {
                await this.referencesRepositoryClient.AddOrUpdateControlReferences(this.projectId, this.projectVersion, controlReference);
            }
            this.controlReferences.AddControlReference(controlReference);
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

    ///<inheritdoc/>  
    public PrefabReferences GetPrefabReferences()
    {
        return this.prefabReferences;
    }

    /// <inheritdoc/>
    public async Task AddPrefabReferenceAsync(PrefabReference prefabReference)
    {
        Guard.Argument(prefabReference, nameof(prefabReference)).NotNull();
        if (!this.prefabReferences.HasReference(prefabReference))
        {
            if (IsOnlineMode)
            {
                await this.referencesRepositoryClient.AddOrUpdatePrefabReferences(this.projectId, this.projectVersion, prefabReference);
            }
            this.prefabReferences.AddPrefabReference(prefabReference);
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

    /// <inheritdoc/>
    public IEnumerable<string> GetAllFixtures()
    {
        return this.projectReferences.Fixtures;
    }

    /// <inheritdoc/>
    public void AddFixture(string fixtureId)
    {
        if (!this.projectReferences.Fixtures.Contains(fixtureId))
        {
            this.projectReferences.Fixtures.Add(fixtureId);
            SaveLocal();
        }
    }

    /// <inheritdoc/>
    public void DeleteFixture(string fixtureId)
    {
        if (this.projectReferences.Fixtures.Contains(fixtureId))
        {
            this.projectReferences.Fixtures.Remove(fixtureId);
            SaveLocal();
        }
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetTestDataSources(string groupKey)
    {
        Guard.Argument(groupKey, nameof(groupKey)).NotNull().NotWhiteSpace();
        var group = this.projectReferences.TestDataSources.FirstOrDefault(t => t.GroupName.Equals(groupKey));
        if(group != null )
        {
            return group.Collection;
        }
        return Enumerable.Empty<string>();
    }

    /// <inheritdoc/>
    public void AddTestDataSource(string groupKey, string testDataSourceId)
    {
        Guard.Argument(groupKey, nameof(groupKey)).NotNull().NotWhiteSpace();
        Guard.Argument(testDataSourceId, nameof(testDataSourceId)).NotNull().NotWhiteSpace();
        var group = this.projectReferences.TestDataSources.FirstOrDefault(t => t.GroupName.Equals(groupKey));
        if (group != null)
        {
            group.Collection.Add(testDataSourceId);
            SaveLocal();
        }
    }

    /// <inheritdoc/>
    public void DeleteTestDataSource(string groupKey, string testDataSourceId)
    {
        Guard.Argument(groupKey, nameof(groupKey)).NotNull().NotWhiteSpace();
        Guard.Argument(testDataSourceId, nameof(testDataSourceId)).NotNull().NotWhiteSpace();
        var group = this.projectReferences.TestDataSources.FirstOrDefault(t => t.GroupName.Equals(groupKey));
        if (group != null)
        {          
            if (group.Collection.Contains(testDataSourceId))
            {
                group.Collection.Remove(testDataSourceId);
                SaveLocal();
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetTestDataSourceGroups()
    {
        return this.projectReferences.TestDataSources.Select(s => s.GroupName);
    }

    /// <inheritdoc/>
    public async Task AddTestDataSourceGroupAsync(string groupKey)
    {
        Guard.Argument(groupKey, nameof(groupKey)).NotNull().NotWhiteSpace();
        if (!this.projectReferences.TestDataSources.ContainsGroup(groupKey))
        {
            if(IsOnlineMode)
            {
                await this.referencesRepositoryClient.AddTestDataSourceGroupAsync(projectId, projectVersion, groupKey);
            }
            this.projectReferences.TestDataSources.Add(new KeyCollectionPair<string>(groupKey));
            SaveLocal();
        }
    }

    /// <inheritdoc/>
    public async Task RenameTestDataSourceGroupAsync(string currentKey, string newKey)
    {
        Guard.Argument(currentKey, nameof(currentKey)).NotNull().NotWhiteSpace();
        Guard.Argument(newKey, nameof(newKey)).NotNull().NotWhiteSpace();       
        if (this.projectReferences.TestDataSources.ContainsGroup(currentKey))
        {
            var group = this.projectReferences.TestDataSources[currentKey];
            if(IsOnlineMode)
            {
                await this.referencesRepositoryClient.RenameTestDataSourceGroupAsync(projectId, projectVersion, currentKey, newKey);
            }
            group.GroupName = newKey;
            SaveLocal();
            return;
        }
        throw new ArgumentException($"Group : '{currentKey}' doesn't exist in the collection");
    }

    /// <inheritdoc/>
    public async Task MoveTestDataSourceToGroupAsync(string testDataSourceId, string currentGroupKey, string newGroupKey)
    {
        Guard.Argument(testDataSourceId, nameof(testDataSourceId)).NotNull().NotWhiteSpace();
        Guard.Argument(currentGroupKey, nameof(currentGroupKey)).NotNull().NotWhiteSpace();
        Guard.Argument(newGroupKey, nameof(newGroupKey)).NotNull().NotWhiteSpace();
        if (!this.projectReferences.TestDataSources.ContainsGroup(currentGroupKey))
        {
            throw new ArgumentException($"Group : '{currentGroupKey}' doesn't exist in the collection");
        }
        if (!this.projectReferences.TestDataSources.ContainsGroup(newGroupKey))
        {
            throw new ArgumentException($"Group : '{newGroupKey}' doesn't exist in the collection");
        }
        var currentGroup = this.projectReferences.TestDataSources[currentGroupKey];
        var newGroup = this.projectReferences.TestDataSources[newGroupKey];
        if(IsOnlineMode)
        {
            await this.referencesRepositoryClient.MoveTestDataSourceToGroupAsync(projectId, projectVersion, testDataSourceId, currentGroupKey, newGroupKey);
        }
        currentGroup.Collection.Remove(testDataSourceId);
        newGroup.Collection.Add(testDataSourceId);
        SaveLocal();
    }


    protected void SaveLocal()
    {
        this.fileSystem.SaveToFile<ProjectReferences>(projectReferences, Path.GetDirectoryName(this.fileSystem.ReferencesFile), Path.GetFileName(this.fileSystem.ReferencesFile));       
    }

    private ProjectReferences CreateDefault()
    {
        var projectReferences = new ProjectReferences();
        projectReferences.TestDataSources.Add(new KeyCollectionPair<string>("Group-1"));
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

