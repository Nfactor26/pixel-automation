using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Components.Processors;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Interfaces.Scripting;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Services.Client;
using Pixel.Scripting.Editor.Core.Contracts;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder
{
    public class AutomationProjectManager : ProjectManager
    {
        private readonly IProjectFileSystem projectFileSystem;
        private readonly IApplicationDataManager applicationDataManager;
        private AutomationProject activeProject;
        private VersionInfo loadedVersion;     

        public AutomationProjectManager(ISerializer serializer, IProjectFileSystem projectFileSystem, ITypeProvider typeProvider,  IScriptEngineFactory scriptEngineFactory, ICodeEditorFactory codeEditorFactory, ICodeGenerator codeGenerator, IApplicationDataManager applicationDataManager) : base(serializer, projectFileSystem, typeProvider, scriptEngineFactory, codeEditorFactory, codeGenerator)
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
            this.entityManager.RegisterDefault<IFileSystem>(this.fileSystem);

            CreateDataModelFile();
            ConfigureCodeEditor();          
            
            this.entityManager.Arguments  = CompileAndCreateDataModel("DataModel");

            Initialize(entityManager, this.activeProject);
            return this.RootEntity;
        }   

        private void CreateDataModelFile()
        {
            string[] dataModelFiles = Directory.GetFiles(this.projectFileSystem.DataModelDirectory, "*.cs");
            if (!dataModelFiles.Any())
            {
                var classGenerator = this.codeGenerator.CreateClassGenerator("DataModel", "Pixel.Automation.Project.DataModels", new[] { typeof(object).Namespace });
                string dataModelInitialContent = classGenerator.GetGeneratedCode();
                File.WriteAllText(Path.Combine(this.fileSystem.DataModelDirectory, "DataModel.cs"), dataModelInitialContent);               
            }             
        }      

        private void Initialize(EntityManager entityManager, AutomationProject automationProject)
        {

            if (!File.Exists(this.projectFileSystem.ProcessFile))
            {
                this.RootEntity = new Entity("Automation Process", "Root");
            }
            else
            {
                this.RootEntity = DeserializeProject();
            }
                       
            AddDefaultEntities();            
        }

        private Entity DeserializeProject()
        {                     
            var entity = this.Load<Entity>(this.projectFileSystem.ProcessFile);         
            return entity;
        }


        private void AddDefaultEntities()
        {
            switch (this.activeProject.ProjectType)
            {
                case ProjectType.ProcessAutomation:
                    if (this.RootEntity.Components.Count() == 0)
                    {
                        var appPoolEntity = new ApplicationPoolEntity();
                        this.RootEntity.AddComponent(appPoolEntity);

                        SequentialEntityProcessor launchProcessor = new SequentialEntityProcessor() { Name = "#Launch Applications" };
                        this.RootEntity.AddComponent(launchProcessor);

                        SequentialEntityProcessor searchProcessor = new SequentialEntityProcessor() { Name = "#Run Automation" };
                        this.RootEntity.AddComponent(searchProcessor);

                        SequentialEntityProcessor resetProcessor = new SequentialEntityProcessor() { Name = "#Reset Applications" };
                        this.RootEntity.AddComponent(resetProcessor);

                        SequentialEntityProcessor shutDownProcessor = new SequentialEntityProcessor() { Name = "#Shutdown Applications" };
                        this.RootEntity.AddComponent(shutDownProcessor);
                    }
                    break;

                case ProjectType.TestAutomation:
                    if (this.RootEntity.Components.Count() == 0)
                    {
                        var appPoolEntity = new ApplicationPoolEntity();
                        this.RootEntity.AddComponent(appPoolEntity);

                        TestFixtureEntity testFixtureEntity = new TestFixtureEntity();
                        this.RootEntity.AddComponent(testFixtureEntity);                      
                    }
                    break;

                default:
                    break;
            }

            RestoreParentChildRelation(RootEntity);
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
        public void Refresh()
        {           
            Debug.Assert(!this.entityManager.RootEntity.GetComponentsOfType<TestCaseEntity>(SearchScope.Descendants).Any());

            this.entityManager.Arguments = CompileAndCreateDataModel("DataModel");
            this.RootEntity.ResetHierarchy();
            serializer.Serialize(this.projectFileSystem.ProcessFile, this.RootEntity, typeProvider.GetAllTypes());            

            //TODO : Since we are discarding existing entities and starting with a fresh copy , any launched applications will be orphaned .
            //We need  a way to restore the state 
            this.RootEntity = DeserializeProject();
            AddDefaultEntities();            
        }

        #region overridden methods

        /// <summary>
        /// Save automation project data
        /// </summary>
        /// <returns></returns>
        public override async Task Save()
        {
            //Remove all the test cases as we don't want them to save as a part of  automamtion process file
            var testCaseEntities = this.entityManager.RootEntity.GetComponentsOfType<TestCaseEntity>(SearchScope.Descendants);
            Entity parentEntity = testCaseEntities.FirstOrDefault()?.Parent;
            foreach (var testEntity in testCaseEntities)
            {
                testEntity.Parent.RemoveComponent(testEntity);
            }
         
            serializer.Serialize(this.projectFileSystem.ProjectFile, this.activeProject);
            this.RootEntity.ResetHierarchy();
            serializer.Serialize(this.projectFileSystem.ProcessFile, this.RootEntity, typeProvider.GetAllTypes());
            await this.applicationDataManager.AddOrUpdateProjectAsync(this.activeProject, this.loadedVersion);

            //Add back the test cases that were already open
            foreach (var testCaseEntity in testCaseEntities)
            {
                parentEntity.AddComponent(testCaseEntity);
            }
        }

        protected override string GetProjectName()
        {
            return this.activeProject.Name;
        }

        #endregion overridden methods
    }
}
