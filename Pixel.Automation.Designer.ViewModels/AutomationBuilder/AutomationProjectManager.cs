using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Components.Processors;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Interfaces.Scripting;
using Pixel.Automation.Core.Models;
using Pixel.Scripting.Editor.Core.Contracts;
using System.IO;
using System.Linq;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder
{
    public class AutomationProjectManager : ProjectManager
    {
        private readonly IProjectFileSystem projectFileSystem;       
        private AutomationProject activeProject;     
        private Entity rootEntity;
        private int compilationIteration = 0;

        public AutomationProjectManager(ISerializer serializer, IProjectFileSystem projectFileSystem, ITypeProvider typeProvider, IScriptEditorFactory scriptEditorFactory, IScriptEngineFactory scriptEngineFactory, ICodeEditorFactory codeEditorFactory, ICodeGenerator codeGenerator) : base(serializer, projectFileSystem, typeProvider, scriptEditorFactory, scriptEngineFactory, codeEditorFactory, codeGenerator)
        {
            this.projectFileSystem = projectFileSystem;   
        }
     

        public Entity Load(AutomationProject activeProject, VersionInfo versionToLoad)
        {
            this.activeProject = activeProject;         
            this.projectFileSystem.Initialize(activeProject.Name, versionToLoad);
            this.entityManager.RegisterDefault<IFileSystem>(this.fileSystem);

            AddDataModelFile();
            ConfigureCodeEditor();          
            this.entityManager.Arguments  = CompileAndCreateDataModel("DataModel");

            Initialize(entityManager, this.activeProject);           
            return this.rootEntity;
        }   

        private void AddDataModelFile()
        {
            string[] dataModelFiles = Directory.GetFiles(this.projectFileSystem.DataModelDirectory, "*.cs");
            if (!dataModelFiles.Any())
            {
                var classGenerator = this.codeGenerator.CreateClassGenerator("DataModel", "Pixel.Automation.Project.DataModels", new[] { typeof(object).Namespace });
                string dataModelInitialContent = classGenerator.GetGeneratedCode();
                File.WriteAllText(Path.Combine(this.fileSystem.DataModelDirectory, "DataModel.cs"), dataModelInitialContent);               
            }             
        }
      
        protected override string GetNewDataModelAssemblyName()
        {
            compilationIteration++;
            string dataModelAssemblyName = $"{this.activeProject.Name.Trim().Replace(' ', '_')}_{compilationIteration}";
            return dataModelAssemblyName;
        }

        private void Initialize(EntityManager entityManager, AutomationProject automationProject)
        {

            if (!File.Exists(this.projectFileSystem.ProcessFile))
            {
                this.rootEntity = new Entity("Automation Process", "Root");
            }
            else
            {
                this.rootEntity = DeserializeProject();
            }

            this.rootEntity.EntityManager = entityManager;
            AddDefaultEntities();
            RestoreParentChildRelation(rootEntity);
        }

        private Entity DeserializeProject()
        {                     
            var entity = this.Load<Entity>(this.projectFileSystem.ProcessFile);         
            return entity;
        }


        private void AddDefaultEntities()
        {
            switch (this.activeProject.AutomationProjectType)
            {
                case ProjectType.ProcessAutomation:
                    if (this.rootEntity.Components.Count() == 0)
                    {
                        var appPoolEntity = new ApplicationPoolEntity();
                        rootEntity.AddComponent(appPoolEntity);

                        SequentialEntityProcessor launchProcessor = new SequentialEntityProcessor() { Name = "#Launch Applications" };
                        rootEntity.AddComponent(launchProcessor);

                        SequentialEntityProcessor searchProcessor = new SequentialEntityProcessor() { Name = "#Run Automation" };
                        rootEntity.AddComponent(searchProcessor);

                        SequentialEntityProcessor resetProcessor = new SequentialEntityProcessor() { Name = "#Reset Applications" };
                        rootEntity.AddComponent(resetProcessor);

                        SequentialEntityProcessor shutDownProcessor = new SequentialEntityProcessor() { Name = "#Shutdown Applications" };
                        rootEntity.AddComponent(shutDownProcessor);
                    }
                    break;

                case ProjectType.TestAutomation:
                    if (this.rootEntity.Components.Count() == 0)
                    {
                        var appPoolEntity = new ApplicationPoolEntity();
                        rootEntity.AddComponent(appPoolEntity);

                        TestFixtureEntity testFixtureEntity = new TestFixtureEntity();
                        rootEntity.AddComponent(testFixtureEntity);                      
                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Save and load project again. Update services to use new data model . One time registration of services is skipped unlike load.
        /// This is required every time data model is compiled and data model has custom types defined. When data model has custom types
        /// it's assembly details are captured during serialization.Also, Arguments refernce the data  model in this case. Hence, we need to 
        /// save and deserialize again so that arguments refer correct assembly.
        /// </summary>
        /// <param name="entityManager"></param>
        /// <returns></returns>
        public Entity Refresh()
        {
            this.entityManager.Arguments = CompileAndCreateDataModel("DataModel");
           
            Save();

            //TODO : Since we are discarding existing entities and starting with a fresh copy , any launched applications will be orphaned .
            //We need  a way to restore the state 
            this.rootEntity = DeserializeProject();
            this.rootEntity.EntityManager = entityManager;

            AddDefaultEntities();
            RestoreParentChildRelation(rootEntity);
            return this.rootEntity;
        }


        public override void Save()
        {
            //Remove all the test cases as we don't want them to save as a part of automation project
            var testCaseEntities = this.entityManager.RootEntity.GetComponentsOfType<TestCaseEntity>(SearchScope.Descendants);
            Entity parentEntity = testCaseEntities.FirstOrDefault()?.Parent;
            foreach (var testEntity in testCaseEntities)
            {
                testEntity.Parent.RemoveComponent(testEntity);
            }
         
            serializer.Serialize(this.projectFileSystem.ProjectFile, this.activeProject);
            this.rootEntity.ResetHierarchy();
            serializer.Serialize(this.projectFileSystem.ProcessFile, this.rootEntity, typeProvider.GetAllTypes());

            //Add back the test cases that were already open
            foreach (var testCaseEntity in testCaseEntities)
            {
                parentEntity.AddComponent(testCaseEntity);
            }
        }


        public override void SaveAs()
        {
            throw new System.NotImplementedException();
        }
       
    }
}
