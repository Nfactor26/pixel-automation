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

            ConfigureCodeEditor();          
            this.entityManager.Arguments  = CompileAndCreateDataModel();

            Initialize(entityManager, this.activeProject);           
            return this.rootEntity;
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
            this.entityManager.Arguments = CompileAndCreateDataModel();
           
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

        /// <summary>
        /// Create a new version of the project and switches file system to use this new version.
        /// Any further changes will be saved to this new version.
        /// </summary>
        public override void CreateSnapShot()
        {
            //save current state to previous version
            //Save();

            ////Increment active version for project
            //Version activeVersion = activeProject.ActiveVersion;
            //Version newVersion = new Version(activeVersion.Major + 1, 0, 0, 0);
            //activeProject.AvailableVersions.Add(newVersion);
            //activeProject.ActiveVersion = newVersion;
        
            ////change file system to new version of project
            //string previousVersionWorkingDirectory = this.projectFileSystem.WorkingDirectory;
            //this.projectFileSystem.SwitchToVersion(this.activeProject.ActiveVersion);
            //string currentWorkingDirectory = this.projectFileSystem.WorkingDirectory;

            ////copy contents from previous version directory to new version directory
            //CopyAll(new DirectoryInfo(previousVersionWorkingDirectory), new DirectoryInfo(currentWorkingDirectory));

            ////save the automation project file as we have added a new version and changed active version to latest.
            //serializer.Serialize<AutomationProject>(this.projectFileSystem.ProjectFile, this.activeProject, null);

            //void CopyAll(DirectoryInfo source, DirectoryInfo target)
            //{            
            //    // Copy each file into the new directory.
            //    foreach (FileInfo fi in source.GetFiles())
            //    {
            //        fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            //    }

            //    // Copy each subdirectory using recursion.
            //    foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            //    {
            //        DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
            //        CopyAll(diSourceSubDir, nextTargetSubDir);
            //    }
            //}
          
        }
    }
}
