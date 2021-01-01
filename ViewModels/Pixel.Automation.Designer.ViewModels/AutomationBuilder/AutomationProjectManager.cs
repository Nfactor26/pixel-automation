using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Components.Processors;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Pixel.Scripting.Editor.Core.Contracts;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder
{
    public class AutomationProjectManager : ProjectManager, IAutomationProjectManager
    {
        private readonly IProjectFileSystem projectFileSystem;    
        private AutomationProject activeProject;
        private VersionInfo loadedVersion;     

        public AutomationProjectManager(ISerializer serializer, IEntityManager entityManager, IProjectFileSystem projectFileSystem, ITypeProvider typeProvider, IArgumentTypeProvider argumentTypeProvider, ICodeEditorFactory codeEditorFactory, IScriptEditorFactory scriptEditorFactory, ICodeGenerator codeGenerator, IApplicationDataManager applicationDataManager) 
        : base(serializer, entityManager, projectFileSystem, typeProvider, argumentTypeProvider, codeEditorFactory, scriptEditorFactory, codeGenerator, applicationDataManager)
        {
            this.projectFileSystem = Guard.Argument(projectFileSystem, nameof(projectFileSystem)).NotNull().Value;
            this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
        }

        #region Load Project

        public async  Task<Entity> Load(AutomationProject activeProject, VersionInfo versionToLoad)
        {
            this.activeProject = activeProject;
            this.loadedVersion = versionToLoad;
            await this.applicationDataManager.DownloadProjectDataAsync(activeProject, versionToLoad);
            this.projectFileSystem.Initialize(activeProject.Name, versionToLoad);
            this.entityManager.SetCurrentFileSystem(this.fileSystem);

            CreateDataModelFile();
            ConfigureCodeEditor();

            this.entityManager.Arguments  = CompileAndCreateDataModel("DataModel");
          
            ConfigureScriptEditor(this.fileSystem); //every time data model assembly changes, we need to reconfigure script editor
            ConfigureArgumentTypeProvider(this.entityManager.Arguments.GetType().Assembly);
            Initialize();
            return this.RootEntity;
        }   

        private void CreateDataModelFile()
        {
            string[] dataModelFiles = Directory.GetFiles(this.projectFileSystem.DataModelDirectory, "*.cs");
            if (!dataModelFiles.Any())
            {
                var classGenerator = this.codeGenerator.CreateClassGenerator("DataModel", $"Pixel.Automation.{this.GetProjectName()}", new[] { typeof(object).Namespace });
                string dataModelInitialContent = classGenerator.GetGeneratedCode();
                string dataModelFile = Path.Combine(this.fileSystem.DataModelDirectory, "DataModel.cs");
                File.WriteAllText(dataModelFile, dataModelInitialContent);
                logger.Information($"Created data model file : {dataModelFile}");
            }             
        }      

        private void Initialize()
        {
            logger.Information($"Loading project file for {this.GetProjectName()} now");
            if (!File.Exists(this.projectFileSystem.ProcessFile))
            {
                this.RootEntity = new Entity("Automation Process", "Root");
            }
            else
            {
                this.RootEntity = DeserializeProject();
            }
                       
            AddDefaultEntities();
            logger.Information($"Project file for {this.GetProjectName()} has ben loaded ");
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
        public override async Task Refresh()
        {           
            Debug.Assert(!this.entityManager.RootEntity.GetComponentsOfType<TestCaseEntity>(SearchScope.Descendants).Any());

            logger.Information($"{this.GetProjectName()} will be re-loaded");
            this.entityManager.Arguments = CompileAndCreateDataModel("DataModel");
            ConfigureScriptEditor(this.fileSystem); //every time data model assembly changes, we need to reconfigure script editor
            ConfigureArgumentTypeProvider(this.entityManager.Arguments.GetType().Assembly);
            this.RootEntity.ResetHierarchy();
            serializer.Serialize(this.projectFileSystem.ProcessFile, this.RootEntity, typeProvider.GetAllTypes());            
                   
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
            serializer.Serialize(this.projectFileSystem.ProcessFile, this.RootEntity, typeProvider.GetAllTypes());
            await this.applicationDataManager.AddOrUpdateProjectAsync(this.activeProject, this.loadedVersion);

            //Add back the test cases that were already open
            foreach (var testFixtureEntity in testFixtureEntities)
            {
                parentEntity.AddComponent(testFixtureEntity);
            }
        }

        protected override string GetProjectName()
        {
            return this.activeProject.GetProjectName();
        }

        #endregion overridden methods
    }
}
