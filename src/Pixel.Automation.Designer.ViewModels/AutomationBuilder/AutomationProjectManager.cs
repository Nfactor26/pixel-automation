using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Components.Entities;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Reference.Manager.Contracts;
using System.IO;

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

        #region Load Project

        public async  Task<Entity> Load(AutomationProject activeProject, VersionInfo versionToLoad)
        {
            this.activeProject = activeProject;
            this.loadedVersion = versionToLoad;
            this.projectFileSystem.Initialize(activeProject, versionToLoad);
            this.projectAssetDataManager.Initialize(activeProject, versionToLoad);
          
            await this.projectDataManager.DownloadProjectDataFilesAsync(activeProject, versionToLoad as ProjectVersion);          
        
            this.referenceManager = this.referenceManagerFactory.CreateReferenceManager(this.activeProject.ProjectId, versionToLoad.ToString(), this.projectFileSystem);
            this.entityManager.RegisterDefault<IReferenceManager>(this.referenceManager);

            foreach(var prefabReference in this.referenceManager.GetPrefabReferences().References)
            {
                await this.prefabDataManager.DownloadPrefabDataAsync(new PrefabProject() { ApplicationId = prefabReference.ApplicationId, PrefabId = prefabReference.PrefabId },
                    prefabReference.Version);
            }

            this.entityManager.SetCurrentFileSystem(this.fileSystem);
            this.entityManager.RegisterDefault<IFileSystem>(this.fileSystem);         
            
            await CreateDataModelFile();
            ConfigureCodeEditor(this.referenceManager);

            var dataModel = CompileAndCreateDataModel(Constants.AutomationProcessDataModelName);
            ConfigureScriptEngine(this.referenceManager, dataModel);
            ConfigureScriptEditor(this.referenceManager, dataModel);         
            this.entityManager.Arguments = dataModel;

            await ExecuteInitializationScript();
            ConfigureArgumentTypeProvider(this.entityManager.Arguments.GetType().Assembly);
            Initialize();
            SetupInitializationScriptProject(dataModel);
            return this.RootEntity;
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

        /// <summary>
        /// Execute the Initialization script for Automation process.
        /// Empty Initialization script is created if script file doesn't exist already.
        /// </summary>
        private async Task ExecuteInitializationScript()
        {
            try
            {
                var fileSystem = this.entityManager.GetCurrentFileSystem();
                var scriptFile = Path.Combine(fileSystem.ScriptsDirectory, Constants.InitializeEnvironmentScript);
                if (!File.Exists(scriptFile))
                {
                    using (var sw = File.CreateText(scriptFile))
                    {
                        sw.WriteLine("//Default Initialization script for automation process");
                        sw.WriteLine();
                        sw.Write("string dataSourceSuffix = string.Empty;");
                    };
                    logger.Information("Created initialization script file : {scriptFile}", scriptFile);
                }
                var scriptEngine = this.entityManager.GetScriptEngine();
                await scriptEngine.ExecuteFileAsync(scriptFile);
                logger.Information("Executed initialization script : {scriptFile}", scriptFile);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to execute Initialization script {Constants.InitializeEnvironmentScript}");               
            }
        }

        private void Initialize()
        {
            logger.Information($"Loading project file for {this.GetProjectName()} now");
            if (!File.Exists(this.projectFileSystem.ProcessFile))
            {
                this.RootEntity = new ProcessRootEntity();
            }
            else
            {
                this.RootEntity = DeserializeProject();
            }
                       
            AddDefaultEntities();
            logger.Information($"Project file for {this.GetProjectName()} has been loaded ");
        }

        private Entity DeserializeProject()
        {                     
            var entity = this.Load<Entity>(this.projectFileSystem.ProcessFile);         
            return entity;
        }


        private void AddDefaultEntities()
        {
            if (this.RootEntity.Components.Count() == 0)
            {
                this.RootEntity.AddComponent(new ApplicationPoolEntity());
                this.RootEntity.AddComponent(new OneTimeSetUpEntity() { Name = "Environment Setup" });
                this.RootEntity.AddComponent(new OneTimeTearDownEntity() { Name = "Environment Teardown" });
            }
            RestoreParentChildRelation(this.RootEntity);
        }

        #endregion Load Project

        /// <summary>
        /// Save and load project again. Update services to use new data model . One time registration of services is skipped unlike load.
        /// This is required every time data model is compiled and data model has custom types defined. When data model has custom types
        /// it's assembly details are captured during serialization.Also, Arguments refernce the data  model in this case. Hence, we need to 
        /// save and deserialize again so that arguments refer correct assembly.
        /// </summary>
        /// <param name="entityManager"></param>
        /// <returns></returns>
        public override async Task Reload()
        {        

            logger.Information($"{this.GetProjectName()} will be re-loaded");
            var reference = this.fileSystem.LoadFile<ProjectReferences>(this.fileSystem.ReferencesFile);
            this.referenceManager.SetProjectReferences(reference);
            var dataModel = CompileAndCreateDataModel(Constants.AutomationProcessDataModelName);
            ConfigureScriptEngine(this.referenceManager, dataModel);
            ConfigureScriptEditor(this.referenceManager, dataModel);          
            this.entityManager.Arguments = dataModel;
            SetupInitializationScriptProject(dataModel);
            await ExecuteInitializationScript();        
            ConfigureArgumentTypeProvider(this.entityManager.Arguments.GetType().Assembly);
            this.RootEntity.ResetHierarchy();
            serializer.Serialize(this.projectFileSystem.ProcessFile, this.RootEntity, typeProvider.GetKnownTypes());            
                   
            var rootEntity = DeserializeProject();
            //we don't want any launched applications to be lost. Copy over ApplicationDetails from each ApplicationEntity in to newly loaded root entity.
            foreach(var applicationEntity in this.entityManager.RootEntity.GetComponentsOfType<ApplicationEntity>(SearchScope.Descendants))
            {
                var newApplicationEntity = rootEntity.GetComponentById(applicationEntity.Id, SearchScope.Descendants) as IApplicationEntity;
                newApplicationEntity.SetTargetApplicationDetails(applicationEntity.GetTargetApplicationDetails());
            }         
            this.RootEntity = rootEntity;
            RestoreParentChildRelation(this.RootEntity);
            await Task.CompletedTask;
            logger.Information($"Reload completed for project {this.GetProjectName()}");

        }

        #region overridden methods

        /// <summary>
        /// Save automation project and process
        /// </summary>
        /// <returns></returns>
        public override async Task Save()
        {
            //Remove all the test fixtures as we don't want them to save as a part of  automamtion process file
            var testFixtureEntities = this.entityManager.RootEntity.GetComponentsOfType<TestFixtureEntity>(SearchScope.Descendants);
            Entity parentEntity = testFixtureEntities.FirstOrDefault()?.Parent;
            foreach (var testEntity in testFixtureEntities)
            {
                testEntity.Parent.RemoveComponent(testEntity);
            }

            serializer.Serialize(this.projectFileSystem.ProjectFile, this.activeProject);
            this.RootEntity.ResetHierarchy();
            serializer.Serialize(this.projectFileSystem.ProcessFile, this.RootEntity, typeProvider.GetKnownTypes());
            
            //Add back the test cases that were already open
            foreach (var testFixtureEntity in testFixtureEntities)
            {
                parentEntity.AddComponent(testFixtureEntity);
            }

            await this.projectDataManager.SaveProjectDataAsync(this.activeProject, this.loadedVersion as ProjectVersion);
        }

        protected override string GetProjectName()
        {
            return this.activeProject.Name;
        }

        protected override string GetProjectNamespace()
        {
            return this.activeProject.Namespace;
        }

        #endregion overridden methods
    }
}
