using Pixel.Automation.Core;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Interfaces.Scripting;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.Processors;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder
{
    public class PrefabProjectManager : ProjectManager
    {
        private readonly IPrefabFileSystem prefabFileSystem;      
    
        private PrefabDescription prefabDescription;
        private Entity rootEntity;
        private Entity prefabbedEntity;
        private int compilationIteration = 0;

        public PrefabProjectManager(ISerializer serializer, IPrefabFileSystem prefabFileSystem, ITypeProvider typeProvider, IScriptEditorFactory scriptEditorFactory, IScriptEngineFactory scriptEngineFactory, ICodeEditorFactory codeEditorFactory, ICodeGenerator codeGenerator) : base(serializer, prefabFileSystem, typeProvider, scriptEditorFactory, scriptEngineFactory, codeEditorFactory, codeGenerator)
        {
            this.prefabFileSystem = prefabFileSystem;
        }
   
        public Entity Load(PrefabDescription prefabDescription, VersionInfo versionInfo)
        {
            this.prefabDescription = prefabDescription;
            this.prefabFileSystem.Initialize(prefabDescription.ApplicationId, prefabDescription.PrefabId, versionInfo);
            this.entityManager.RegisterDefault<IFileSystem>(this.fileSystem);

            Assembly mostRecentAssembly = this.prefabFileSystem.GetDataModelAssembly();
            this.compilationIteration = int.Parse(mostRecentAssembly.GetName().Name.Split(new char[] { '_' }).LastOrDefault() ?? "0");

            ConfigureCodeEditor();
            this.entityManager.Arguments = CompileAndCreateDataModel();
            Initialize(this.entityManager, this.prefabDescription);
            return this.rootEntity;
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
            this.Save();
            this.Initialize(this.entityManager, this.prefabDescription);
            return this.rootEntity;
        }


        private void Initialize(EntityManager entityManager, PrefabDescription prefabDescription)
        {
            if (!File.Exists(this.prefabFileSystem.PrefabFile))
            {
                throw new FileNotFoundException();
            }         
            this.rootEntity = AddEntitiesAndPrefab();        
            
        }

        private Entity DeserializePrefab()
        {           
            var entity = this.Load<Entity>(this.prefabFileSystem.PrefabFile);
            return entity;
        }

        private Entity AddEntitiesAndPrefab()
        {
            this.prefabbedEntity = DeserializePrefab();

            Entity templateRoot = this.prefabFileSystem.GetTemplate();
            if (templateRoot != null)
            {
                templateRoot.EntityManager = this.entityManager;
                RestoreParentChildRelation(templateRoot);
                Entity prefabPlaceHolder = templateRoot.GetFirstComponentOfType<SequentialProcessorEntity>();
                prefabPlaceHolder.AddComponent(this.prefabbedEntity);
                RestoreParentChildRelation(prefabPlaceHolder);
            }
            else
            {
                templateRoot = new Entity("Automation Process", "Root");
                templateRoot.EntityManager = entityManager;

                ApplicationPoolEntity appPoolEntity = new ApplicationPoolEntity();
                templateRoot.AddComponent(appPoolEntity);

                SequentialProcessorEntity launchProcessor = new SequentialProcessorEntity() { Name = "#Launch Applications" };
                templateRoot.AddComponent(launchProcessor);

                SequentialProcessorEntity prefabProcessor = new SequentialProcessorEntity() { Name = "#Prefab Processor" };
                templateRoot.AddComponent(prefabProcessor);

                SequentialProcessorEntity shutDownProcessor = new SequentialProcessorEntity() { Name = "#Shutdown Applications" };
                templateRoot.AddComponent(shutDownProcessor);              

                this.prefabFileSystem.CreateOrReplaceTemplate(templateRoot);

                prefabProcessor.AddComponent(prefabbedEntity);               
                RestoreParentChildRelation(rootEntity);
            }

            return templateRoot;
        }

        /// <summary>
        /// Save any changes to Prefab
        /// </summary>
        public override void Save()
        {
            this.rootEntity.ResetHierarchy();
            serializer.Serialize(this.prefabFileSystem.PrefabFile, this.prefabbedEntity, typeProvider.GetAllTypes());

            var prefabParent = this.prefabbedEntity.Parent;
            prefabParent.RemoveComponent(this.prefabbedEntity);
            this.prefabFileSystem.CreateOrReplaceTemplate(this.rootEntity);
            prefabParent.AddComponent(this.prefabbedEntity);
        }

        public override void SaveAs()
        {
            throw new NotImplementedException();
        }

        public override void CreateSnapShot()
        {
            //save current state to previous version
            //Save();

            ////Increment active version for project
            //VersionInfo activeVersion = prefabDescription.ActiveVersion;
            //VersionInfo newVersion = new VersionInfo(new Version(activeVersion.Version.Major + 1, 0, 0, 0));
            //prefabDescription.SetActiveVersion(newVersion);

            ////change file system to new version of project
            //string previousVersionWorkingDirectory = this.prefabFileSystem.WorkingDirectory;
            //this.prefabFileSystem.SwitchToVersion(newVersion);
            //string currentWorkingDirectory = this.prefabFileSystem.WorkingDirectory;

            ////copy contents from previous version directory to new version directory
            //CopyAll(new DirectoryInfo(previousVersionWorkingDirectory), new DirectoryInfo(currentWorkingDirectory));         

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

        protected override string GetNewDataModelAssemblyName()
        {
            compilationIteration++;
            string dataModelAssemblyName = $"{this.prefabDescription.PrefabName.Trim().Replace(' ', '_')}_{compilationIteration}";
            return dataModelAssemblyName;
        }
    }
}
