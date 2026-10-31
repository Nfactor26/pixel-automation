using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using System.Diagnostics;
using System.IO;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder;

/// <summary>
/// Manager for a <see cref="PrefabProject"/>
/// </summary>
public class PrefabProjectManager : ProjectManager, IPrefabProjectManager
{
    private readonly IPrefabFileSystem prefabFileSystem;
    private readonly IReferenceManagerFactory referenceManagerFactory;
    private readonly IReferenceProviderFactory referenceProviderFactory;
    private readonly IPrefabDataManager prefabDataManager;      
    private PrefabProject prefabProject;
    private VersionInfo loadedVersion;           
         
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="serializer">Serializer</param>
    /// <param name="entityManager">EntityManager</param>
    /// <param name="prefabFileSystem">Prefab file system</param>
    /// <param name="typeProvider">Known Type provider</param>
    /// <param name="argumentTypeProvider">Argument type provider</param>
    /// <param name="codeEditorFactory">Factory for creading code editorss</param>
    /// <param name="scriptEditorFactory">Factory for creating script editors</param>
    /// <param name="scriptEngineFactory">Factory for creating script enginess</param>
    /// <param name="codeGenerator">Code generator for generating initial data model code</param>
    /// <param name="applicationDataManager">Data manager for application data</param>
    /// <param name="prefabDataManager">Data manager for prefab data</param>
    /// <param name="applicationSettings">Application settings</param>
    /// <param name="referenceManagerFactory">Factory for creating Reference Manager</param>
    public PrefabProjectManager(ISerializer serializer, IEntityManager entityManager, IPrefabFileSystem prefabFileSystem, ITypeProvider typeProvider, IArgumentTypeProvider argumentTypeProvider,
        ICodeEditorFactory codeEditorFactory, IScriptEditorFactory scriptEditorFactory, IScriptEngineFactory scriptEngineFactory,
        ICodeGenerator codeGenerator, IApplicationDataManager applicationDataManager, IPrefabDataManager prefabDataManager, ApplicationSettings applicationSettings,
        IReferenceManagerFactory referenceManagerFactory, IReferenceProviderFactory referenceProviderFactory)
        : base(serializer, entityManager, prefabFileSystem, typeProvider, argumentTypeProvider, codeEditorFactory, scriptEditorFactory, scriptEngineFactory, codeGenerator, applicationDataManager)
    {
        this.prefabFileSystem = Guard.Argument(prefabFileSystem, nameof(prefabFileSystem)).NotNull().Value;            
        this.referenceManagerFactory = Guard.Argument(referenceManagerFactory, nameof(referenceManagerFactory)).NotNull().Value;
        this.referenceProviderFactory = Guard.Argument(referenceProviderFactory, nameof(referenceProviderFactory)).NotNull().Value;
        this.prefabDataManager = Guard.Argument(prefabDataManager, nameof(prefabDataManager)).NotNull().Value;
    }

    #region load project

    public async Task<Entity> Load(PrefabProject prefabProject, VersionInfo versionToLoad)
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(Load), ActivityKind.Internal))
        {
            Guard.Argument(prefabProject, nameof(prefabProject)).NotNull();
            Guard.Argument(versionToLoad, nameof(versionToLoad)).NotNull();
          
            this.prefabProject = prefabProject;
            this.loadedVersion = versionToLoad;
            this.prefabFileSystem.Initialize(prefabProject, versionToLoad);

            await this.prefabDataManager.DownloadPrefabDataAsync(this.prefabProject, this.loadedVersion);
          
            this.entityManager.SetIdentifier($"Design Prefab : {prefabProject.Name}");
            
            this.entityManager.SetCurrentFileSystem(this.fileSystem);
            this.entityManager.RegisterDefault<IFileSystem>(this.fileSystem);

            var referenceProvider = referenceProviderFactory.CreateReferenceProvider(this.fileSystem);
            this.entityManager.SetRefereceProvider(referenceProvider);

            this.referenceManager = referenceManagerFactory.CreateReferenceManager(this.prefabProject.ProjectId, versionToLoad.ToString(), this.prefabFileSystem);
            this.entityManager.RegisterDefault<IReferenceManager>(this.referenceManager);

            await CreateDataModelFile();
            ConfigureCodeEditor(this.referenceManager);

            var dataModel = CompileAndCreateDataModel(Constants.PrefabDataModelName);
            ConfigureScriptEngine(this.referenceManager, dataModel);
            ConfigureScriptEditor(this.referenceManager, dataModel);
            this.entityManager.Arguments = dataModel;
                         
            ConfigureArgumentTypeProvider(this.entityManager.Arguments.GetType().Assembly);
            Initialize();
            SetupInitializationScriptProject(dataModel);
            await ExecuteInitializationScript(executeDefaultInitFunc: false);

            await OnProjectLoaded(prefabProject, versionToLoad);
            return this.RootEntity;
        }           
    }

    private async Task CreateDataModelFile()
    {
        string[] dataModelFiles = Directory.GetFiles(this.prefabFileSystem.DataModelDirectory, "*.cs");
        if (!dataModelFiles.Any())
        {
            var classGenerator = this.codeGenerator.CreateClassGenerator(Constants.PrefabDataModelName, this.GetProjectNamespace(), new[] { typeof(object).Namespace });
            string dataModelInitialContent = classGenerator.GetGeneratedCode();
            string dataModelFile = Path.Combine(this.fileSystem.DataModelDirectory, $"{Constants.PrefabDataModelName}.cs");
            await File.WriteAllTextAsync(dataModelFile, dataModelInitialContent);
            logger.Information($"Created data model file : {dataModelFile}");
        }
    }

    private void Initialize()
    {
        if (!File.Exists(this.prefabFileSystem.PrefabFile))
        {
            //TODO : Add Application references to script editors
            //var targetApplication = applicationDataManager.GetAllApplications().FirstOrDefault(a => a.ApplicationId.Equals(prefabProject.ApplicationId));
            //var applicationSequence = new SequenceEntity()
            //{
            //    Name = $"Sequence : {targetApplication.ApplicationName}",
            //    TargetAppId = targetApplication.ApplicationId
            //};
            this.prefabFileSystem.SaveToFile<Entity>(new PrefabEntity() {  Name = this.prefabProject.Name}, Path.GetDirectoryName(this.prefabFileSystem.PrefabFile), Path.GetFileName(this.prefabFileSystem.PrefabFile));
        }
        this.RootEntity = this.Load<Entity>(this.prefabFileSystem.PrefabFile);


    } 

    #endregion load project

    /// <summary>
    /// Save and load project again. Update services to use new data model . One time registration of services is skipped unlike load.
    /// This is required every time data model is compiled and data model has custom types defined. When data model has custom types
    /// it's assembly details are captured during serialization.Also, Arguments refernce the data  model in this case. Hence, we need to 
    /// save and deserialize again so that arguments refer correct assembly.
    /// </summary>
    /// <param name="entityManager"></param>
    /// <returns></returns>
    public override async Task Reload(EditorReferences existing, EditorReferences updated)
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(Reload), ActivityKind.Internal))
        {
            await this.Save();

            this.codeEditorFactory.RemoveProject(GetProjectName());
            this.codeEditorFactory.RemoveAssemblyReferences(existing.CodeEditorReferences.ToArray());

            this.RootEntity.DisposeEditors();

            this.scriptEditorFactory.RemoveProject(this.prefabProject.ProjectId);
            this.scriptEditorFactory.RemoveAssemblyReference(this.entityManager.Arguments.GetType().Assembly);
            this.scriptEditorFactory.RemoveAssemblyReferences(existing.ScriptEditorReferences.ToArray());
            this.scriptEditorFactory.RemoveImports(existing.ScriptImports.ToArray());

            this.scriptEngineFactory.RemoveReferences(this.entityManager.Arguments.GetType().Assembly);
            this.scriptEngineFactory.RemoveReferences(existing.ScriptEngineReferences.ToArray());
            this.scriptEngineFactory.RemoveNamespaces(existing.ScriptImports.ToArray());

            var reference = this.fileSystem.LoadFile<ProjectReferences>(this.fileSystem.ReferencesFile);
            this.referenceManager.SetProjectReferences(reference);

            this.codeEditorFactory.AddAssemblyReferences(updated.CodeEditorReferences.ToArray());
            var dataModel = CompileAndCreateDataModel(Constants.PrefabDataModelName);
            this.entityManager.Arguments = dataModel; // Setting up a new model will also configure script engine to use new assembly

            this.scriptEditorFactory.AddAssemblyReference(dataModel.GetType().Assembly);
            this.scriptEditorFactory.AddAssemblyReferences(updated.ScriptEditorReferences.ToArray());
            this.scriptEditorFactory.AddImports(updated.ScriptImports.ToArray());

            //TODO : If a reference was white listed earlier, we need to remove it
            this.scriptEngineFactory.WithAdditionalAssemblyReferences(this.entityManager.Arguments.GetType().Assembly)
                .WithAdditionalAssemblyReferences(updated.ScriptEngineReferences.ToArray())
                .WithAdditionalNamespaces(updated.ScriptImports.ToArray())
                .WithWhiteListedReferences(updated.WhiteListedReferences);

            this.ConfigureArgumentTypeProvider(this.entityManager.Arguments.GetType().Assembly);

            this.Initialize();
            this.SetupInitializationScriptProject(dataModel);
            await this.ExecuteInitializationScript(executeDefaultInitFunc: true);              

            logger.Information($"Prefab project: '{this.GetProjectName()}' was reloaded");
        }                  
    }


    #region overridden methods

    protected override void SetupInitializationScriptProject(object dataModel)
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(SetupInitializationScriptProject), ActivityKind.Internal))
        {
            this.scriptEditorFactory.AddProject(this.prefabProject.ProjectId, Array.Empty<string>(), dataModel.GetType());
            var scriptFile = Path.Combine(fileSystem.ScriptsDirectory, Constants.InitializeEnvironmentScript);
            CreateInitializationScriptFile(scriptFile);
            this.scriptEditorFactory.AddDocument(scriptFile, this.prefabProject.ProjectId, this.fileSystem.ReadAllText(scriptFile));
        }
    }

    protected override void CreateInitializationScriptFile(string scriptFile)
    {
        if (!File.Exists(scriptFile))
        {
            using var sw = File.CreateText(scriptFile);              
            logger.Information("Created initialization script file : {scriptFile}", scriptFile);
        }
    }

    ///<inheritdoc/>
    public override async Task DownloadFileByNameAsync(string fileName)
    {
        await this.prefabDataManager.DownloadPrefabDataFileByNameAsync(this.prefabProject, this.loadedVersion, fileName);
    }

    ///<inheritdoc/>
    public override async Task AddOrUpdateDataFileAsync(string targetFile)
    {
        await this.prefabDataManager.AddOrUpdateDataFileAsync(this.prefabProject, this.loadedVersion, targetFile, this.prefabProject.ProjectId);
    }

    ///<inheritdoc/>
    public override async Task DeleteDataFileAsync(string fileToDelete)
    {
        await this.prefabDataManager.DeleteDataFileAsync(this.prefabProject, this.loadedVersion, fileToDelete);
    }

    ///<inheritdoc/>
    public override async Task DownloadDataModelFilesAsync()
    {
        await this.prefabDataManager.DownloadDataModelFilesAsync(this.prefabProject, this.loadedVersion);
    }
    /// <inheritdoc/>       
    protected override string GetProjectName()
    {
        return this.prefabProject.Name;
    }

    /// <inheritdoc/>   
    protected override string GetProjectNamespace()
    {
        return this.prefabProject.Namespace;
    }

    /// <summary>
    /// Save prefab data
    /// </summary>
    public override async Task Save()
    {
        try
        {
            this.RootEntity.ResetHierarchy();               
            this.prefabFileSystem.SaveToFile<Entity>(this.RootEntity, Path.GetDirectoryName(this.prefabFileSystem.PrefabFile), Path.GetFileName(this.prefabFileSystem.PrefabFile));
          
            //when saving a published version, we need to recompile the data model assembly and save in references directory
            if (this.loadedVersion.IsPublished)
            {
                CompileDataModelAssemblyForVersion(this.loadedVersion.Version);
            }

            await this.prefabDataManager.SavePrefabDataAsync(this.prefabProject, this.loadedVersion);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "There was an error while trying to save Prefab");
        }
    }

   
    #endregion overridden methods
}
