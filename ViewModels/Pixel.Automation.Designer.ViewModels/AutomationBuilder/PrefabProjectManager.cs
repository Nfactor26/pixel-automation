using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Components.Processors;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Interfaces.Scripting;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using System.IO;
using System.Threading.Tasks;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder
{
    public class PrefabProjectManager : ProjectManager, IPrefabProjectManager
    {
        private readonly IPrefabFileSystem prefabFileSystem;      
    
        private PrefabDescription prefabDescription;       
        private Entity prefabbedEntity;       

        public PrefabProjectManager(ISerializer serializer, IEntityManager entityManager, IPrefabFileSystem prefabFileSystem, ITypeProvider typeProvider, IArgumentTypeProvider argumentTypeProvider, ICodeEditorFactory codeEditorFactory, IScriptEditorFactory scriptEditorFactory, ICodeGenerator codeGenerator)
            : base(serializer, entityManager, prefabFileSystem, typeProvider, argumentTypeProvider, codeEditorFactory, scriptEditorFactory, codeGenerator)
        {
            this.prefabFileSystem = Guard.Argument(prefabFileSystem, nameof(prefabFileSystem)).NotNull().Value;
        }

        #region load project

        public Entity Load(PrefabDescription prefabDescription, VersionInfo versionInfo)
        {
            this.prefabDescription = prefabDescription;
            this.prefabFileSystem.Initialize(prefabDescription.ApplicationId, prefabDescription.PrefabId, versionInfo);
            this.entityManager.SetCurrentFileSystem(this.fileSystem);

            ConfigureCodeEditor();
          
            this.entityManager.Arguments = CompileAndCreateDataModel("PrefabDataModel");

            ConfigureScriptEditor(this.fileSystem); //every time data model assembly changes, we need to reconfigure script editor
            ConfigureArgumentTypeProvider(this.entityManager.Arguments.GetType().Assembly);
            Initialize();           
            return this.RootEntity;
        }
    

        private void Initialize()
        {
            if (!File.Exists(this.prefabFileSystem.PrefabFile))
            {
                throw new FileNotFoundException();
            }         
            this.RootEntity = AddEntitiesAndPrefab();        
            
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

        #endregion load project

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
            await this.Save();
            this.Initialize();
            this.entityManager.Arguments = CompileAndCreateDataModel("PrefabDataModel");
            ConfigureScriptEditor(this.fileSystem); //every time data model assembly changes, we need to reconfigure script editor
            ConfigureArgumentTypeProvider(this.entityManager.Arguments.GetType().Assembly);          
        }


        #region overridden methods

        /// <summary>
        /// Save prefab data
        /// </summary>
        public override async Task Save()
        {
            this.RootEntity.ResetHierarchy();
            serializer.Serialize(this.prefabFileSystem.PrefabFile, this.prefabbedEntity, typeProvider.GetAllTypes());

            var prefabParent = this.prefabbedEntity.Parent;
            prefabParent.RemoveComponent(this.prefabbedEntity);
            this.prefabFileSystem.CreateOrReplaceTemplate(this.RootEntity);
            prefabParent.AddComponent(this.prefabbedEntity);
            await Task.CompletedTask;
        }


        protected override string GetProjectName()
        {
            return this.prefabDescription.PrefabName;
        }

        #endregion overridden methods
    }
}
