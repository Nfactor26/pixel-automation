using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Components.Entities;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using System.Diagnostics;
using System.IO;
using Pixel.Scripting.Editor.Core;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder
{
    public class AutomationProjectManager : ProjectManager, IAutomationProjectManager
    {
        private readonly IProjectFileSystem projectFileSystem;
        private readonly IReferenceManagerFactory referenceManagerFactory;
        private readonly IProjectDataManager projectDataManager;
        private readonly IPrefabDataManager prefabDataManager;
        private readonly IProjectAssetsDataManager projectAssetDataManager;
        private AutomationProject activeProject;
        private VersionInfo loadedVersion;     

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="entityManager"></param>
        /// <param name="projectFileSystem"></param>
        /// <param name="typeProvider"></param>
        /// <param name="argumentTypeProvider"></param>
        /// <param name="codeEditorFactory"></param>
        /// <param name="scriptEditorFactory"></param>
        /// <param name="scriptEngineFactory"></param>
        /// <param name="codeGenerator"></param>
        /// <param name="applicationDataManager"></param>
        /// <param name="referenceManagerFactory"></param>
        /// <param name="projectDataManager"></param>
        /// <param name="prefabDataManager"></param>
        /// <param name="projectAssetDataManager"></param>
        public AutomationProjectManager(ISerializer serializer, IEntityManager entityManager, IProjectFileSystem projectFileSystem, ITypeProvider typeProvider, IArgumentTypeProvider argumentTypeProvider,
            ICodeEditorFactory codeEditorFactory, IScriptEditorFactory scriptEditorFactory, IScriptEngineFactory scriptEngineFactory, ICodeGenerator codeGenerator, IApplicationDataManager applicationDataManager,
            IReferenceManagerFactory referenceManagerFactory, IProjectDataManager projectDataManager, IPrefabDataManager prefabDataManager, IProjectAssetsDataManager projectAssetDataManager) 
        : base(serializer, entityManager, projectFileSystem, typeProvider, argumentTypeProvider, codeEditorFactory, scriptEditorFactory, scriptEngineFactory, codeGenerator, applicationDataManager)
        {
            this.projectFileSystem = Guard.Argument(projectFileSystem, nameof(projectFileSystem)).NotNull().Value;          
            this.referenceManagerFactory = Guard.Argument(referenceManagerFactory, nameof(referenceManagerFactory)).NotNull().Value;
            this.projectDataManager = Guard.Argument(projectDataManager, nameof(projectDataManager)).NotNull().Value;
            this.projectAssetDataManager = Guard.Argument(projectAssetDataManager, nameof(projectAssetDataManager)).NotNull().Value;
            this.prefabDataManager = Guard.Argument(prefabDataManager, nameof(prefabDataManager)).NotNull().Value;
        }

        #region methods

        public async  Task<Entity> Load(AutomationProject activeProject, VersionInfo versionToLoad)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(Load), ActivityKind.Internal))
            {
                Guard.Argument(activeProject, nameof(activeProject)).NotNull();
                Guard.Argument(versionToLoad, nameof(versionToLoad)).NotNull();
              
                this.activeProject = activeProject;
                this.loadedVersion = versionToLoad;
                this.projectFileSystem.Initialize(activeProject, versionToLoad);
                this.projectAssetDataManager.Initialize(activeProject, versionToLoad);

                await this.projectDataManager.DownloadProjectDataFilesAsync(activeProject, versionToLoad);

                this.referenceManager = this.referenceManagerFactory.CreateReferenceManager(this.activeProject.ProjectId, versionToLoad.ToString(), this.projectFileSystem);
                this.entityManager.RegisterDefault<IReferenceManager>(this.referenceManager);

                foreach (var prefabReference in this.referenceManager.GetPrefabReferences().References)
                {
                    await this.prefabDataManager.DownloadPrefabDataAsync(prefabReference.ApplicationId, prefabReference.PrefabId, prefabReference.Version.ToString());
                }

                this.entityManager.SetCurrentFileSystem(this.fileSystem);
                this.entityManager.RegisterDefault<IFileSystem>(this.fileSystem);

                await CreateDataModelFile();
                ConfigureCodeEditor(this.referenceManager);

                var dataModel = CompileAndCreateDataModel(Constants.AutomationProcessDataModelName);
                ConfigureScriptEngine(this.referenceManager, dataModel);
                ConfigureScriptEditor(this.referenceManager, dataModel);
                this.entityManager.Arguments = dataModel;
                               
                ConfigureArgumentTypeProvider(this.entityManager.Arguments.GetType().Assembly);
                Initialize();
                SetupInitializationScriptProject(dataModel);
                await ExecuteInitializationScript(executeDefaultInitFunc: true);
             
                await OnProjectLoaded(activeProject, versionToLoad);
                return this.RootEntity;
            }           
        }   

        private async Task CreateDataModelFile()
        {
            string[] dataModelFiles = Directory.GetFiles(this.projectFileSystem.DataModelDirectory, "*.cs");
            if (!dataModelFiles.Any())
            {
                var classGenerator = this.codeGenerator.CreateClassGenerator(Constants.AutomationProcessDataModelName, this.GetProjectNamespace(), new[] { typeof(object).Namespace });
                string dataModelInitialContent = classGenerator.GetGeneratedCode();
                string dataModelFile = Path.Combine(this.fileSystem.DataModelDirectory, $"{Constants.AutomationProcessDataModelName}.cs");
                await File.WriteAllTextAsync(dataModelFile, dataModelInitialContent);
                logger.Information($"Created data model file : {dataModelFile}");
            }             
        }

        protected override void CreateInitializationScriptFile(string scriptFile)
        {
            if (!File.Exists(scriptFile))
            {
                using (var sw = File.CreateText(scriptFile))
                {
                    sw.WriteLine("string dataSourceSuffix = string.Empty;");
                    sw.WriteLine();
                    sw.WriteLine("//Default Initialization function");
                    sw.Write("void ");
                    sw.Write(Constants.DefaultInitFunction);
                    sw.WriteLine("{");
                    sw.WriteLine();
                    sw.WriteLine("}");
                };
                logger.Information("Created initialization script file : {scriptFile}", scriptFile);
            }
        }
        
        private void Initialize()
        {
            logger.Information($"Loading project file for {this.GetProjectName()} now");
            if (!File.Exists(this.projectFileSystem.ProcessFile))
            {
                this.RootEntity = new ProcessRootEntity();
                this.RootEntity.AddComponent(new ApplicationPoolEntity());
                this.RootEntity.AddComponent(new OneTimeSetUpEntity() { Name = "Environment Setup" });
                this.RootEntity.AddComponent(new OneTimeTearDownEntity() { Name = "Environment Teardown" });
                this.serializer.Serialize<Entity>(this.projectFileSystem.ProcessFile, this.RootEntity);
            }
            else
            {
                this.RootEntity = DeserializeProject();
            }
            RestoreParentChildRelation(this.RootEntity);

            logger.Information($"Project file for {this.GetProjectName()} has been loaded ");
        }

        private Entity DeserializeProject()
        {                     
            var entity = this.Load<Entity>(this.projectFileSystem.ProcessFile);         
            return entity;
        }
   
        public void OnPrefabVersionChanged(IEnumerable<PrefabReference> prefabs)
        {
            var prefabLoader = this.entityManager.GetServiceOfType<IPrefabLoader>();
            foreach (var prefab in prefabs)
            {
                if(prefabLoader.IsPrefabLoaded(prefab.PrefabId))
                {
                    //Remove loaded assembly from script editor and script engine
                    var prefabDataModel = prefabLoader.GetPrefabDataModelType(prefab.ApplicationId, prefab.PrefabId, this.entityManager);
                    this.scriptEditorFactory.RemoveAssemblyReference(prefabDataModel.Assembly);
                    this.scriptEngineFactory.RemoveReferences(prefabDataModel.Assembly);
                    
                    //unload and dispose the prefab. Subsequent use will load the recent version again and 
                    //configure script editor and script engine
                    prefabLoader.UnloadAndDispose(prefab.PrefabId);
                }              
            }
        }

        /// <summary>
        /// Save and load project again. Update services to use new data model . One time registration of services is skipped unlike load.
        /// This is required every time data model is compiled and data model has custom types defined. When data model has custom types
        /// it's assembly details are captured during serialization. Also, Arguments refernce the data  model in this case. Hence, we need to 
        /// save and deserialize again so that arguments refer correct assembly.
        /// </summary>
        /// <param name="entityManager"></param>
        /// <returns></returns>
        public override async Task Reload(EditorReferences existing, EditorReferences updated)
        {
            Guard.Argument(existing, nameof(existing)).NotNull();
            Guard.Argument(updated, nameof(updated)).NotNull();

            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(Reload), ActivityKind.Internal))
            {
                await this.Save();

                this.codeEditorFactory.RemoveProject(GetProjectName());
                this.codeEditorFactory.RemoveAssemblyReferences(existing.CodeEditorReferences.ToArray());

                //Dispose any inline editors that might be present and clean up script editor and script engine
                this.RootEntity.DisposeEditors();

                this.scriptEditorFactory.RemoveProject(RootEntity.Id);
                this.scriptEditorFactory.RemoveAssemblyReference(this.entityManager.Arguments.GetType().Assembly);
                this.scriptEditorFactory.RemoveAssemblyReferences(existing.ScriptEditorReferences.ToArray());
                this.scriptEditorFactory.RemoveImports(existing.ScriptImports.ToArray());

                this.scriptEngineFactory.RemoveReferences(this.entityManager.Arguments.GetType().Assembly);
                this.scriptEngineFactory.RemoveReferences(existing.ScriptEngineReferences.ToArray());
                this.scriptEngineFactory.RemoveNamespaces(existing.ScriptImports.ToArray());
                
                var reference = this.fileSystem.LoadFile<ProjectReferences>(this.fileSystem.ReferencesFile);
                this.referenceManager.SetProjectReferences(reference);

                this.codeEditorFactory.AddAssemblyReferences(updated.CodeEditorReferences.ToArray());
                var dataModel = CompileAndCreateDataModel(Constants.AutomationProcessDataModelName);              
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
               
                //we don't want any launched applications to be lost. Copy over ApplicationDetails from each ApplicationEntity in to newly loaded root entity.
                foreach (var applicationEntity in this.entityManager.RootEntity.GetComponentsOfType<ApplicationEntity>(SearchScope.Descendants))
                {
                    var newApplicationEntity = this.RootEntity.GetComponentById(applicationEntity.Id, SearchScope.Descendants) as IApplicationEntity;
                    newApplicationEntity.SetTargetApplicationDetails(applicationEntity.GetTargetApplicationDetails());
                }              
                             
                logger.Information($"Project : '{this.GetProjectName()}' was reloaded");
            }
        }

        #endregion methods

        #region overridden methods

        ///<inheritdoc/>
        public override async Task DownloadFileByNameAsync(string fileName)
        {
            await this.projectDataManager.DownloadProjectDataFileByNameAsync(this.activeProject, this.loadedVersion, fileName);
        }

        ///<inheritdoc/>
        public override async Task AddOrUpdateDataFileAsync(string targetFile)
        {
            await this.projectDataManager.AddOrUpdateDataFileAsync(this.activeProject, this.loadedVersion, targetFile, this.activeProject.ProjectId);
        }

        ///<inheritdoc/>
        public override async Task DeleteDataFileAsync(string fileToDelete)
        {
            await this.projectDataManager.DeleteDataFileAsync(this.activeProject, this.loadedVersion, fileToDelete);
        }

        ///<inheritdoc/>
        public override async Task DownloadDataModelFilesAsync()
        {
            await this.projectDataManager.DownloadDataModelFilesAsync(this.activeProject, this.loadedVersion);
        }

        ///<inheritdoc/>
        protected override string GetProjectName()
        {
            return this.activeProject.Name;
        }

        ///<inheritdoc/>
        protected override string GetProjectNamespace()
        {
            return this.activeProject.Namespace;
        }

        /// <summary>
        /// Save automation project and process
        /// </summary>
        /// <returns></returns>
        public override async Task Save()
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(Save), ActivityKind.Internal))
            {
                //Remove all the test fixtures as we don't want them to save as a part of  automamtion process file
                var testFixtureEntities = this.entityManager.RootEntity.GetComponentsOfType<TestFixtureEntity>(SearchScope.Descendants);
                Entity parentEntity = testFixtureEntities.FirstOrDefault()?.Parent;
                foreach (var testFixtureEntity in testFixtureEntities)
                {
                    //we don't want to dispose test fixture entity and/or clear out it's entity manager as we need to restore it back soon.
                    testFixtureEntity.Parent.RemoveComponent(testFixtureEntity, false);
                }

                serializer.Serialize(this.projectFileSystem.ProjectFile, this.activeProject);
                this.RootEntity.ResetHierarchy();
                serializer.Serialize(this.projectFileSystem.ProcessFile, this.RootEntity, typeProvider.GetKnownTypes());

                //Add back the test cases that were already open
                foreach (var testFixtureEntity in testFixtureEntities)
                {
                    parentEntity.AddComponent(testFixtureEntity);
                }

                //when saving a published version, we need to recompile the data model assembly and save in references directory
                if(this.loadedVersion.IsPublished)
                {
                    CompileDataModelAssemblyForVersion(this.loadedVersion.Version);
                }

                await this.projectDataManager.SaveProjectDataAsync(this.activeProject, this.loadedVersion);
            }            
        }

        #endregion overridden methods
    }
}
