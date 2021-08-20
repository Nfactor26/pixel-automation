using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Persistence.Services.Client;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder
{
    public class PrefabBuilderViewModel : Wizard
    {
        private readonly ILogger logger = Log.ForContext<PrefabBuilderViewModel>();

        private readonly IPrefabFileSystem prefabFileSystem;
        private readonly ICodeGenerator codeGenerator;
        private readonly ICodeEditorFactory codeEditorFactory;
        private readonly IScriptEngineFactory scriptEngineFactory;
        private readonly ISerializer serializer;
        private readonly IApplicationDataManager applicationDataManager;

        private PrefabProject prefabToolBoxItem;

        public PrefabBuilderViewModel(ISerializer serializer, IApplicationDataManager applicationDataManager,
            IPrefabFileSystem prefabFileSystem, ICodeGenerator codeGenerator, ICodeEditorFactory codeEditorFactory, IScriptEngineFactory scriptEngineFactory)
        {
            this.DisplayName = "Prefab Builder";
            this.prefabFileSystem = prefabFileSystem;
            this.codeGenerator = codeGenerator;
            this.codeEditorFactory = codeEditorFactory;
            this.scriptEngineFactory = scriptEngineFactory;
            this.serializer = serializer;
            this.applicationDataManager = applicationDataManager;
        }

        public void Initialize(ApplicationDescription applicationItem, Entity rootEntity)
        {
            logger.Information($"Create prefab initiated by user for application : {applicationItem.ApplicationName}");

            this.stagedScreens.Clear();
            PrefabVersion prefabVersion = new PrefabVersion(new Version(1, 0, 0, 0));
            prefabToolBoxItem = new PrefabProject()
            {
                ApplicationId = applicationItem.ApplicationId,
                PrefabId = Guid.NewGuid().ToString(),            
                AvailableVersions = new List<PrefabVersion>() { prefabVersion },
                PrefabRoot = rootEntity
            };
                    
            //we don't have assembly name initially until project is compiled. We don't need it anyways while building prefab.
            prefabFileSystem.Initialize(applicationItem.ApplicationId, prefabToolBoxItem.PrefabId, prefabVersion);

            var prefabToolBoxViewModel = new NewPrefabViewModel(applicationItem, prefabToolBoxItem);
            this.stagedScreens.Add(prefabToolBoxViewModel);

            //we need refrence to ScriptEngine in use by EntityManager so that we can extract declared script variables here
            IScriptEngine entityScriptEngine = rootEntity.EntityManager.GetScriptEngine();
            var prefabDataModelBuilderViewModel = new PrefabDataModelBuilderViewModel(prefabToolBoxItem, codeGenerator,
                this.prefabFileSystem, entityScriptEngine, new CompositeTypeExtractor(), new ArgumentExtractor());
            this.stagedScreens.Add(prefabDataModelBuilderViewModel);
       
            var references = new AssemblyReferences().GetReferencesOrDefault();            
            this.codeEditorFactory.Initialize(prefabFileSystem.DataModelDirectory, references.ToArray());  
            var prefabDataModelEditorViewModel = new PrefabDataModelEditorViewModel(this.prefabToolBoxItem, this.prefabFileSystem, this.codeEditorFactory);
            this.stagedScreens.Add(prefabDataModelEditorViewModel);
         
            var prefabScriptImporterViewModel = new PrefabScriptsImporterViewModel(prefabToolBoxItem, rootEntity, 
                new ScriptExtractor(new ArgumentExtractor()), new ArgumentExtractor(), prefabFileSystem, scriptEngineFactory);
            this.stagedScreens.Add(prefabScriptImporterViewModel);

            prefabToolBoxViewModel.NextScreen = prefabDataModelBuilderViewModel;
            prefabDataModelBuilderViewModel.PreviousScreen = prefabToolBoxViewModel;
            prefabDataModelBuilderViewModel.NextScreen = prefabDataModelEditorViewModel;
            prefabDataModelEditorViewModel.PreviousScreen = prefabDataModelBuilderViewModel;
            prefabDataModelEditorViewModel.NextScreen = prefabScriptImporterViewModel;
            prefabScriptImporterViewModel.PreviousScreen = prefabDataModelEditorViewModel;

        }

        public async Task<PrefabProject> SavePrefabAsync()
        {
            serializer.Serialize<PrefabProject>(prefabFileSystem.PrefabDescriptionFile, prefabToolBoxItem);
            serializer.Serialize<Entity>(prefabFileSystem.PrefabFile, prefabToolBoxItem.PrefabRoot as Entity);
            UpdateAssemblyReferenceAndNameSpace(prefabToolBoxItem, prefabFileSystem);
            logger.Information($"Created new prefab : {this.prefabToolBoxItem.PrefabName}");
           
            await this.applicationDataManager.AddOrUpdatePrefabAsync(prefabToolBoxItem, prefabFileSystem.ActiveVersion);
            await this.applicationDataManager.AddOrUpdatePrefabDataFilesAsync(prefabToolBoxItem, prefabFileSystem.ActiveVersion);           
            return this.prefabToolBoxItem;
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
            var oldAssembly = prefabProject.PrefabRoot.EntityManager.Arguments.GetType()
                .Assembly.GetName();
            var oldNameSpace = prefabProject.PrefabRoot.EntityManager.Arguments.GetType().Namespace;
            var newAssemblyName = prefabProject.GetPrefabName();
            string fileContents = File.ReadAllText(prefabFileSystem.PrefabFile);
            Regex assmelbyNameMatcher = new Regex($"{oldAssembly.Name}");
            fileContents = assmelbyNameMatcher.Replace(fileContents, (m) =>
            {
                // while loading prefab regex matches with format assemblyname_digit in data file to update with most recent assembly.
                // Hence, we append _0 so that Regex matches as expected.
                return $"{newAssemblyName}_0";
            });
            fileContents = fileContents.Replace(oldNameSpace, prefabProject.NameSpace);
            File.WriteAllText(prefabFileSystem.PrefabFile, fileContents);

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
