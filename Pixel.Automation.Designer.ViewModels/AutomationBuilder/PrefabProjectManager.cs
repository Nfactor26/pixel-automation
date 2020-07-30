using Pixel.Automation.Core;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Components.Processors;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Interfaces.Scripting;
using Pixel.Automation.Core.Models;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder
{
    public class PrefabProjectManager : ProjectManager
    {
        private readonly IPrefabFileSystem prefabFileSystem;      
    
        private PrefabDescription prefabDescription;
        private Entity rootEntity;
        private Entity prefabbedEntity;      

        public PrefabProjectManager(ISerializer serializer, IPrefabFileSystem prefabFileSystem, ITypeProvider typeProvider, IScriptEngineFactory scriptEngineFactory, ICodeEditorFactory codeEditorFactory, ICodeGenerator codeGenerator) : base(serializer, prefabFileSystem, typeProvider, scriptEngineFactory, codeEditorFactory, codeGenerator)
        {
            this.prefabFileSystem = prefabFileSystem;
        }
   
        public Entity Load(PrefabDescription prefabDescription, VersionInfo versionInfo)
        {
            this.prefabDescription = prefabDescription;
            this.prefabFileSystem.Initialize(prefabDescription.ApplicationId, prefabDescription.PrefabId, versionInfo);
            this.entityManager.RegisterDefault<IFileSystem>(this.fileSystem);
                
            ConfigureCodeEditor();
            this.entityManager.Arguments = CompileAndCreateDataModel("PrefabDataModel");
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
        public async Task<Entity> Refresh()
        {
            this.entityManager.Arguments = CompileAndCreateDataModel("PrefabDataModel");      
            await this.Save();
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

        private Entity AddEntitiesAndPrefab()
        {
            this.prefabbedEntity = this.Load<Entity>(this.prefabFileSystem.PrefabFile);
            Entity templateRoot = default;
            
            if(File.Exists(this.prefabFileSystem.TemplateFile))
            {
                templateRoot = this.Load<Entity>(this.prefabFileSystem.TemplateFile); ;
            }           

            if (templateRoot != null)
            {
                templateRoot.EntityManager = this.entityManager;
                RestoreParentChildRelation(templateRoot);
                Entity prefabPlaceHolder = templateRoot.GetFirstComponentOfType<SequentialEntityProcessor>();
                prefabPlaceHolder.AddComponent(this.prefabbedEntity);
                RestoreParentChildRelation(prefabPlaceHolder);
            }
            else
            {
                templateRoot = new Entity("Automation Process", "Root");
                templateRoot.EntityManager = entityManager;

                ApplicationPoolEntity appPoolEntity = new ApplicationPoolEntity();
                templateRoot.AddComponent(appPoolEntity);

                SequentialEntityProcessor launchProcessor = new SequentialEntityProcessor() { Name = "#Launch Applications" };
                templateRoot.AddComponent(launchProcessor);

                SequentialEntityProcessor prefabProcessor = new SequentialEntityProcessor() { Name = "#Prefab Processor" };
                templateRoot.AddComponent(prefabProcessor);

                SequentialEntityProcessor shutDownProcessor = new SequentialEntityProcessor() { Name = "#Shutdown Applications" };
                templateRoot.AddComponent(shutDownProcessor);              

                this.prefabFileSystem.CreateOrReplaceTemplate(templateRoot);

                prefabProcessor.AddComponent(prefabbedEntity);               
                RestoreParentChildRelation(templateRoot);
            }

            return templateRoot;
        }

        /// <summary>
        /// Save any changes to Prefab
        /// </summary>
        public override async Task Save()
        {
            this.rootEntity.ResetHierarchy();
            serializer.Serialize(this.prefabFileSystem.PrefabFile, this.prefabbedEntity, typeProvider.GetAllTypes());

            var prefabParent = this.prefabbedEntity.Parent;
            prefabParent.RemoveComponent(this.prefabbedEntity);
            this.prefabFileSystem.CreateOrReplaceTemplate(this.rootEntity);
            prefabParent.AddComponent(this.prefabbedEntity);
            await Task.CompletedTask;
        }

        public override void SaveAs()
        {
            throw new NotImplementedException();
        }

        protected override string GetProjectName()
        {
            return this.prefabDescription.PrefabName;
        }
    }
}
