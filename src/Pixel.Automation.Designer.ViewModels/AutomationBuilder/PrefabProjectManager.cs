using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Components.Entities;
using Pixel.Automation.Core.Components.Processors;
using Pixel.Automation.Core.Components.Sequences;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using System.Diagnostics;
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
        private VersionInfo loadedVersion;
        private Entity prefabEntity;       
             
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="serializer">Serializer</param>
        /// <param name="entityManager">EntityManager</param>
        /// <param name="prefabFileSystem">Prefab file system</param>
        /// <param name="typeProvider">Known Type provider</param>
        /// <param name="argumentTypeProvider">Argument type provider</param>
        /// <param name="codeEditorFactory">Factory for creading code editorss</param>
        /// <param name="scriptEditorFactory">Factory for creating script editors</param>
        /// <param name="scriptEngineFactory">Factory for creating script enginess</param>
        /// <param name="codeGenerator">Code generator for generating initial data model code</param>
        /// <param name="applicationDataManager">Data manager for application data</param>
        /// <param name="prefabDataManager">Data manager for prefab data</param>
        /// <param name="applicationSettings">Application settings</param>
        /// <param name="referenceManagerFactory">Factory for creating Reference Manager</param>
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
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(Load), ActivityKind.Internal))
            {
                Guard.Argument(prefabProject, nameof(prefabProject)).NotNull();
                Guard.Argument(versionToLoad, nameof(versionToLoad)).NotNull();
              
                this.prefabProject = prefabProject;
                this.loadedVersion = versionToLoad;
                this.prefabFileSystem.Initialize(prefabProject, versionToLoad);

                await this.prefabDataManager.DownloadPrefabDataAsync(this.prefabProject, this.loadedVersion);

                this.entityManager.SetCurrentFileSystem(this.fileSystem);
                this.entityManager.RegisterDefault<IFileSystem>(this.fileSystem);
                this.referenceManager = referenceManagerFactory.CreateReferenceManager(this.prefabProject.ProjectId, versionToLoad.ToString(), this.prefabFileSystem);
                this.entityManager.RegisterDefault<IReferenceManager>(this.referenceManager);

                ConfigureCodeEditor(this.referenceManager);

                var dataModel = CompileAndCreateDataModel(Constants.PrefabDataModelName);
                ConfigureScriptEngine(this.referenceManager, dataModel);
                ConfigureScriptEditor(this.referenceManager, dataModel);
                this.entityManager.Arguments = dataModel;

                await ExecuteInitializationScript(executeDefaultInitFunc: false);
                ConfigureArgumentTypeProvider(this.entityManager.Arguments.GetType().Assembly);
                Initialize();
                SetupInitializationScriptProject(dataModel);
                await OnProjectLoaded(prefabProject, versionToLoad);
                return this.RootEntity;
            }           
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
            this.prefabEntity = this.Load<Entity>(this.prefabFileSystem.PrefabFile);     
            var targetApplication = applicationDataManager.GetAllApplications().FirstOrDefault(a => a.ApplicationId.Equals(prefabProject.ApplicationId));

            if (!File.Exists(this.prefabFileSystem.TemplateFile))
            {
                var templateRoot = new ProcessRootEntity();
                templateRoot.EntityManager = entityManager;

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

                SequentialEntityProcessor prefabProcessor = new SequentialEntityProcessor() { Name = "Run Prefab", Tag = "RunPrefab" };
                templateRoot.AddComponent(prefabProcessor);
                prefabProcessor.AddComponent(new SequenceEntity()
                {
                    Name = $"Initialize Data",
                    TargetAppId = applicationEntity.ApplicationId
                });
                this.prefabFileSystem.CreateOrReplaceTemplate(templateRoot);
            }           

            var root = this.Load<Entity>(this.prefabFileSystem.TemplateFile);
            root.EntityManager = this.entityManager;
            RestoreParentChildRelation(root);
            var runPrefabProcessor = root.GetComponentsByTag("RunPrefab").Single() as Entity;

            //In order to run prefab, it must belong to a SequenceEntity. We can create a prefab from any entity.
            //Hence, we check if the prefab root is not a SequenceEntity, we create a placeholder SequenceEntity first and add prefab entity to it.
            if(this.prefabEntity is SequenceEntity)
            {
                runPrefabProcessor.AddComponent(this.prefabEntity);
            }
            else
            {
                var applicationSequence = new SequenceEntity()
                {
                    Name = $"Sequence : {targetApplication.ApplicationName}",
                    TargetAppId = targetApplication.ApplicationId
                };
                runPrefabProcessor.AddComponent(applicationSequence);
                applicationSequence.AddComponent(this.prefabEntity);
            }          
            RestoreParentChildRelation(runPrefabProcessor);        
            return root;
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
                applicationEntity.ApplicationFile = Path.Combine(applicationSettings.ApplicationDirectory, targetApplication.ApplicationId, $"{targetApplication.ApplicationId}.app").Replace("\\", "/");
            
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
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(Reload), ActivityKind.Internal))
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
        }


        #region overridden methods

        protected override void CreateInitializationScriptFile(string scriptFile)
        {
            if (!File.Exists(scriptFile))
            {
                using var sw = File.CreateText(scriptFile);              
                logger.Information("Created initialization script file : {scriptFile}", scriptFile);
            }
        }

        ///<inheritdoc/>
        public override async Task DownloadFileByNameAsync(string fileName)
        {
            await this.prefabDataManager.DownloadPrefabDataFileByNameAsync(this.prefabProject, this.loadedVersion, fileName);
        }

        ///<inheritdoc/>
        public override async Task AddOrUpdateDataFileAsync(string targetFile)
        {
            await this.prefabDataManager.AddOrUpdateDataFileAsync(this.prefabProject, this.loadedVersion, targetFile, this.prefabProject.ProjectId);
        }

        ///<inheritdoc/>
        public override async Task DeleteDataFileAsync(string fileToDelete)
        {
            await this.prefabDataManager.DeleteDataFileAsync(this.prefabProject, this.loadedVersion, fileToDelete);
        }

        ///<inheritdoc/>
        public override async Task DownloadDataModelFilesAsync()
        {
            await this.prefabDataManager.DownloadDataModelFilesAsync(this.prefabProject, this.loadedVersion);
        }
        /// <inheritdoc/>       
        protected override string GetProjectName()
        {
            return this.prefabProject.Name;
        }

        /// <inheritdoc/>   
        protected override string GetProjectNamespace()
        {
            return this.prefabProject.Namespace;
        }

        /// <summary>
        /// Save prefab data
        /// </summary>
        public override async Task Save()
        {
            try
            {
                this.RootEntity.ResetHierarchy();
                serializer.Serialize(this.prefabFileSystem.PrefabFile, this.prefabEntity, typeProvider.GetKnownTypes());
                if(this.prefabEntity is SequenceEntity sequenceEntity)
                {
                    var prefabProcessor = sequenceEntity.Parent;
                    prefabProcessor.RemoveComponent(sequenceEntity);
                    this.prefabFileSystem.CreateOrReplaceTemplate(this.RootEntity);
                    prefabProcessor.AddComponent(this.prefabEntity);

                }
                else
                {
                    var prefabProcessor = this.prefabEntity.Parent.Parent;
                    prefabProcessor.RemoveComponent(this.prefabEntity.Parent);
                    this.prefabFileSystem.CreateOrReplaceTemplate(this.RootEntity);
                    prefabProcessor.AddComponent(this.prefabEntity.Parent);
                }

                //when saving a published version, we need to recompile the data model assembly and save in references directory
                if (this.loadedVersion.IsPublished)
                {
                    CompileDataModelAssemblyForVersion(this.loadedVersion.Version);
                }

                await this.prefabDataManager.SavePrefabDataAsync(this.prefabProject, this.loadedVersion);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to save Prefab");
            }
        }

       
        #endregion overridden methods
    }
}
