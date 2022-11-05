using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Components.Entities;
using Pixel.Automation.Core.Components.Processors;
using Pixel.Automation.Core.Components.Sequences;
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
    /// <summary>
    /// Manager for a <see cref="PrefabProject"/>
    /// </summary>
    public class PrefabProjectManager : ProjectManager, IPrefabProjectManager
    {
        private readonly IPrefabFileSystem prefabFileSystem;
        private readonly IReferenceManagerFactory referenceManagerFactory;
        private readonly IPrefabDataManager prefabDataManager;
        private readonly ApplicationSettings applicationSettings;
        private PrefabProject prefabProject;
        private PrefabVersion loadedVersion;
        private Entity prefabbedEntity;       
             
        public PrefabProjectManager(ISerializer serializer, IEntityManager entityManager, IPrefabFileSystem prefabFileSystem, ITypeProvider typeProvider, IArgumentTypeProvider argumentTypeProvider,
            ICodeEditorFactory codeEditorFactory, IScriptEditorFactory scriptEditorFactory, IScriptEngineFactory scriptEngineFactory,
            ICodeGenerator codeGenerator, IApplicationDataManager applicationDataManager, IPrefabDataManager prefabDataManager, ApplicationSettings applicationSettings,
            IReferenceManagerFactory referenceManagerFactory)
            : base(serializer, entityManager, prefabFileSystem, typeProvider, argumentTypeProvider, codeEditorFactory, scriptEditorFactory, scriptEngineFactory, codeGenerator, applicationDataManager)
        {
            this.prefabFileSystem = Guard.Argument(prefabFileSystem, nameof(prefabFileSystem)).NotNull().Value;
            this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull().Value;
            this.referenceManagerFactory = Guard.Argument(referenceManagerFactory, nameof(referenceManagerFactory)).NotNull().Value;
            this.prefabDataManager = Guard.Argument(prefabDataManager, nameof(prefabDataManager)).NotNull().Value;
        }

        #region load project

        public async Task<Entity> Load(PrefabProject prefabProject, VersionInfo versionToLoad)
        {
            this.prefabProject = prefabProject;
            this.loadedVersion = versionToLoad as PrefabVersion;
            this.prefabFileSystem.Initialize(prefabProject, versionToLoad);

            await this.prefabDataManager.DownloadPrefabDataAsync(this.prefabProject, this.loadedVersion);

            this.entityManager.SetCurrentFileSystem(this.fileSystem);
            this.entityManager.RegisterDefault<IFileSystem>(this.fileSystem);
            this.referenceManager = referenceManagerFactory.CreateReferenceManager(this.prefabProject.PrefabId, versionToLoad.ToString(), this.prefabFileSystem);
            this.entityManager.RegisterDefault<IReferenceManager>(this.referenceManager);         

            ConfigureCodeEditor(this.referenceManager);

            var dataModel = CompileAndCreateDataModel(Constants.PrefabDataModelName);
            ConfigureScriptEngine(this.referenceManager, dataModel);
            ConfigureScriptEditor(this.referenceManager, dataModel);          
            this.entityManager.Arguments = dataModel;
            ConfigureArgumentTypeProvider(this.entityManager.Arguments.GetType().Assembly);
            Initialize();          
            return await Task.FromResult(this.RootEntity);
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
                Entity prefabPlaceHolder = templateRoot.GetComponentsOfType<SequentialEntityProcessor>(SearchScope.Children).Last();
                prefabPlaceHolder.AddComponent(this.prefabbedEntity);
                RestoreParentChildRelation(prefabPlaceHolder);
            }
            else
            {
                templateRoot = new ProcessRootEntity();
                templateRoot.EntityManager = entityManager;

                var targetApplication = applicationDataManager.GetAllApplications().FirstOrDefault(a => a.ApplicationId.Equals(prefabProject.ApplicationId));

                ApplicationPoolEntity appPoolEntity = new ApplicationPoolEntity();          
                templateRoot.AddComponent(appPoolEntity);

                var applicationEntity = CreateApplicationEntity(targetApplication);
                appPoolEntity.AddComponent(applicationEntity);

                SequentialEntityProcessor launchProcessor = new SequentialEntityProcessor() { Name = "Launch Applications" };
                templateRoot.AddComponent(launchProcessor);
                launchProcessor.AddComponent(new SequenceEntity() 
                {
                    Name = $"Sequence : {targetApplication.ApplicationName}",
                    TargetAppId = applicationEntity.ApplicationId 
                });
               

                SequentialEntityProcessor shutDownProcessor = new SequentialEntityProcessor() { Name = "Shutdown Applications" };
                templateRoot.AddComponent(shutDownProcessor);
                shutDownProcessor.AddComponent(new SequenceEntity()
                {
                    Name = $"Sequence : {targetApplication.ApplicationName}",
                    TargetAppId = applicationEntity.ApplicationId
                });

                SequentialEntityProcessor prefabProcessor = new SequentialEntityProcessor() { Name = "Run Prefab" };                           
                templateRoot.AddComponent(prefabProcessor);
                prefabProcessor.AddComponent(new SequenceEntity()
                {
                    Name = $"Sequence : {targetApplication.ApplicationName}",
                    TargetAppId = applicationEntity.ApplicationId
                });

                var applicationSequence = prefabProcessor.Components.First() as Entity;                            
                applicationSequence.AddComponent(new SequenceEntity() { Name = "Initialize Data" });
            
                this.prefabFileSystem.CreateOrReplaceTemplate(templateRoot);

                applicationSequence.AddComponent(prefabbedEntity);               
                RestoreParentChildRelation(templateRoot);
            }

            return templateRoot;
        }

        private ApplicationEntity CreateApplicationEntity(ApplicationDescription targetApplication)        {
           
            ApplicationEntityAttribute applicationEntityAttribute = targetApplication.ApplicationDetails.GetType().GetCustomAttributes(typeof(ApplicationEntityAttribute), false).FirstOrDefault()
                                                                      as ApplicationEntityAttribute;
            if (applicationEntityAttribute != null)
            {
                var applicationEntity = Activator.CreateInstance(applicationEntityAttribute.ApplicationEntity) as ApplicationEntity;
                applicationEntity.Name = $"Details : {targetApplication.ApplicationName}";
                applicationEntity.ApplicationId = targetApplication.ApplicationId;
                applicationEntity.EntityManager = this.entityManager;
                applicationEntity.ApplicationFile = Path.Combine(applicationSettings.ApplicationDirectory, targetApplication.ApplicationId, $"{targetApplication.ApplicationId}.app");
            
                applicationEntity.GetTargetApplicationDetails();
                return applicationEntity;
            }
            throw new Exception($"Failed to create application entity for application id : {prefabProject.ApplicationId}");
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
        public override async Task Reload()
        {
            await this.Save();
            this.Initialize();                      
            var reference = this.fileSystem.LoadFile<ProjectReferences>(this.fileSystem.ReferencesFile);
            this.referenceManager.SetProjectReferences(reference);
            var dataModel = CompileAndCreateDataModel(Constants.PrefabDataModelName);
            ConfigureScriptEngine(this.referenceManager, dataModel);
            ConfigureScriptEditor(this.referenceManager, dataModel);           
            this.entityManager.Arguments = dataModel;           
            ConfigureArgumentTypeProvider(this.entityManager.Arguments.GetType().Assembly);          
        }


        #region overridden methods

        /// <summary>
        /// Save prefab data
        /// </summary>
        public override async Task Save()
        {
            try
            {
                this.RootEntity.ResetHierarchy();
                serializer.Serialize(this.prefabFileSystem.PrefabFile, this.prefabbedEntity, typeProvider.GetKnownTypes());

                var prefabParent = this.prefabbedEntity.Parent;
                prefabParent.RemoveComponent(this.prefabbedEntity);
                this.prefabFileSystem.CreateOrReplaceTemplate(this.RootEntity);
                prefabParent.AddComponent(this.prefabbedEntity);

                //push the changes to database           
                await this.prefabDataManager.SavePrefabDataAsync(this.prefabProject, this.loadedVersion);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to save Prefab");
            }
        }


        protected override string GetProjectName()
        {
            return this.prefabProject.PrefabName;
        }

        protected override string GetProjectNamespace()
        {
            return this.prefabProject.Namespace;
        }

        #endregion overridden methods
    }
}
