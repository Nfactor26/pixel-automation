using Dawn;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Contracts;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Reference.Manager.Contracts;
using Serilog;
using System.IO;
using System.Text.RegularExpressions;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder
{
    public class PrefabBuilderViewModel : Wizard, IPrefabBuilder
    {
        private readonly ILogger logger = Log.ForContext<PrefabBuilderViewModel>();

        private readonly IPrefabFileSystem prefabFileSystem;
        private readonly ICodeGenerator codeGenerator;
        private readonly ICodeEditorFactory codeEditorFactory;
        private readonly IScriptEngineFactory scriptEngineFactory;
        private readonly ISerializer serializer;
        private readonly IApplicationDataManager applicationDataManager;
        private readonly IPrefabDataManager prefabDataManager;
        private readonly IReferenceManagerFactory referenceManagerFactory;
        private IReferenceManager referenceManager;

        private PrefabProject prefabProject;

        public PrefabBuilderViewModel(ISerializer serializer, IApplicationDataManager applicationDataManager, IPrefabDataManager prefabDataManager,
            IPrefabFileSystem prefabFileSystem, ICodeGenerator codeGenerator, ICodeEditorFactory codeEditorFactory,
            IScriptEngineFactory scriptEngineFactory, IReferenceManagerFactory referenceManagerFactory)
        {
            this.DisplayName = "(1/4) Create a new Prefab";
            this.prefabFileSystem = prefabFileSystem;
            this.codeGenerator = codeGenerator;
            this.codeEditorFactory = codeEditorFactory;
            this.scriptEngineFactory = scriptEngineFactory;
            this.serializer = serializer;
            this.applicationDataManager = applicationDataManager;
            this.prefabDataManager = prefabDataManager;
            this.referenceManagerFactory = Guard.Argument(referenceManagerFactory).NotNull().Value;
        }

        public void Initialize(ApplicationDescriptionViewModel applicationDescriptionViewModel, Entity rootEntity)
        {
            logger.Information($"Create prefab initiated by user for application : {applicationDescriptionViewModel.ApplicationName}");

            this.stagedScreens.Clear();
            PrefabVersion prefabVersion = new PrefabVersion(new Version(1, 0, 0, 0));
            prefabProject = new PrefabProject()
            {
                ApplicationId = applicationDescriptionViewModel.ApplicationId,
                PrefabId = Guid.NewGuid().ToString(),            
                AvailableVersions = new List<PrefabVersion>() { prefabVersion },
                PrefabRoot = rootEntity
            };
                    
            //we don't have assembly name initially until project is compiled. We don't need it anyways while building prefab.
            prefabFileSystem.Initialize(prefabProject, prefabVersion);

            var newPreafabViewModel = new NewPrefabViewModel(applicationDescriptionViewModel, prefabProject);
            this.stagedScreens.Add(newPreafabViewModel);

            //we need refrence to ScriptEngine in use by EntityManager so that we can extract declared script variables here
            IScriptEngine entityScriptEngine = rootEntity.EntityManager.GetScriptEngine();
            var prefabDataModelBuilderViewModel = new PrefabDataModelBuilderViewModel(prefabProject, codeGenerator,
                this.prefabFileSystem, entityScriptEngine, new CompositeTypeExtractor(), new ArgumentExtractor());
            this.stagedScreens.Add(prefabDataModelBuilderViewModel);

            this.referenceManager = this.referenceManagerFactory.CreateReferenceManager(prefabProject.PrefabId, prefabVersion.ToString(), this.prefabFileSystem);                
            this.codeEditorFactory.Initialize(prefabFileSystem.DataModelDirectory, referenceManager.GetCodeEditorReferences(), Enumerable.Empty<string>());  
            var prefabDataModelEditorViewModel = new PrefabDataModelEditorViewModel(this.prefabProject, this.prefabFileSystem, this.codeEditorFactory);
            this.stagedScreens.Add(prefabDataModelEditorViewModel);
         
            var prefabScriptImporterViewModel = new PrefabScriptsImporterViewModel(prefabProject, rootEntity, 
                new ScriptExtractor(new ArgumentExtractor()), new ArgumentExtractor(), prefabFileSystem, scriptEngineFactory, referenceManager);
            this.stagedScreens.Add(prefabScriptImporterViewModel);

            newPreafabViewModel.NextScreen = prefabDataModelBuilderViewModel;
            prefabDataModelBuilderViewModel.PreviousScreen = newPreafabViewModel;
            prefabDataModelBuilderViewModel.NextScreen = prefabDataModelEditorViewModel;
            prefabDataModelEditorViewModel.PreviousScreen = prefabDataModelBuilderViewModel;
            prefabDataModelEditorViewModel.NextScreen = prefabScriptImporterViewModel;
            prefabScriptImporterViewModel.PreviousScreen = prefabDataModelEditorViewModel;

        }

        public async Task<PrefabProject> SavePrefabAsync()
        {
            serializer.Serialize<PrefabProject>(prefabFileSystem.PrefabDescriptionFile, prefabProject);
            serializer.Serialize<Entity>(prefabFileSystem.PrefabFile, prefabProject.PrefabRoot as Entity);
            UpdateAssemblyReferenceAndNameSpace(prefabProject, prefabFileSystem);
            await UpdateControlReferencesFileAsync(prefabProject.PrefabRoot as Entity);
            logger.Information($"Created new prefab : {this.prefabProject.PrefabName}");
           
            await this.prefabDataManager.AddPrefabAsync(prefabProject);                
            return this.prefabProject;
        }

        /// <summary>
        /// Entity used to create prefab has references to old assembly and when serialized , data has
        /// namespace and assembly name from old assembly. While generating prefab, we have created new
        /// data models (mirrored types) in to a local assembly for Prefab. We need to replace old references
        /// with new ones while saving. 
        /// </summary>
        /// <param name="prefabProject"></param>
        /// <param name="prefabFileSystem"></param>
        private void UpdateAssemblyReferenceAndNameSpace(PrefabProject prefabProject, IPrefabFileSystem prefabFileSystem)
        {
            //In absence of a custom data model, there is no need to update anything
            if (prefabProject.PrefabRoot.EntityManager.Arguments.GetType() == typeof(EmptyModel))
            {
                return;
            }
            var oldAssembly = prefabProject.PrefabRoot.EntityManager.Arguments.GetType()
                .Assembly.GetName();
            var oldNameSpace = prefabProject.PrefabRoot.EntityManager.Arguments.GetType().Namespace;
            var newAssemblyName = prefabProject.Namespace;
            string fileContents = File.ReadAllText(prefabFileSystem.PrefabFile);
            Regex assmelbyNameMatcher = new Regex($"{oldAssembly.Name}");
            fileContents = assmelbyNameMatcher.Replace(fileContents, (m) =>
            {
                // while loading prefab regex matches with format assemblyname_digit in data file to update with most recent assembly.
                // Hence, we append _0 so that Regex matches as expected.
                return $"{newAssemblyName}_0";
            });
            fileContents = fileContents.Replace(oldNameSpace, prefabProject.Namespace);
            File.WriteAllText(prefabFileSystem.PrefabFile, fileContents);

        }

        /// <summary>
        /// Prefabs can use controls in the automation. We need to create a ControlReferences file to include details of these controls.
        /// By default, we create the references with the most recent version of the control irrespective of what version was it created with.
        /// When a prefab is used in a automation project, control references are merged back in automation project control references.
        /// However, the automation project control references will always decide what version of control is used.
        /// </summary>
        /// <param name="prefabRoot"></param>
        private async Task UpdateControlReferencesFileAsync(Entity prefabRoot)
        {          
            //TODO : Batch addition of controls in a single call
            var referencedControls = prefabRoot.GetComponentsOfType<ControlEntity>(Core.Enums.SearchScope.Descendants);
            foreach(var control in referencedControls)
            {
                var mostRecentVersionOfControl = this.applicationDataManager.GetControlsById(control.ApplicationId, control.ControlId).OrderBy(a => a.Version).Last();
                await this.referenceManager.AddControlReferenceAsync(new ControlReference(mostRecentVersionOfControl.ApplicationId, mostRecentVersionOfControl.ControlId, mostRecentVersionOfControl.Version));
            }            
        }

        public override async Task Cancel()
        {
            string prefabsDirectory = Directory.GetParent(prefabFileSystem.WorkingDirectory).FullName;
            try
            {
                if (Directory.Exists(prefabsDirectory))
                {
                    Directory.Delete(prefabsDirectory, true);
                }
            }
            catch (Exception)
            {
                //When cancelling at a later stage when assembly is generated and loaded , there will be exception deleting folder
            }
            await base.Cancel();
            logger.Information($"Created new prefab operation was cancelled");
        }

    }
}
