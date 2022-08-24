﻿using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Contracts;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;
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
    public class PrefabBuilderViewModel : Wizard, IPrefabBuilder
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

        public void Initialize(ApplicationDescriptionViewModel applicationDescriptionViewModel, Entity rootEntity)
        {
            logger.Information($"Create prefab initiated by user for application : {applicationDescriptionViewModel.ApplicationName}");

            this.stagedScreens.Clear();
            PrefabVersion prefabVersion = new PrefabVersion(new Version(1, 0, 0, 0));
            prefabToolBoxItem = new PrefabProject()
            {
                ApplicationId = applicationDescriptionViewModel.ApplicationId,
                PrefabId = Guid.NewGuid().ToString(),            
                AvailableVersions = new List<PrefabVersion>() { prefabVersion },
                PrefabRoot = rootEntity
            };
                    
            //we don't have assembly name initially until project is compiled. We don't need it anyways while building prefab.
            prefabFileSystem.Initialize(prefabToolBoxItem, prefabVersion);

            var newPreafabViewModel = new NewPrefabViewModel(applicationDescriptionViewModel, prefabToolBoxItem);
            this.stagedScreens.Add(newPreafabViewModel);

            //we need refrence to ScriptEngine in use by EntityManager so that we can extract declared script variables here
            IScriptEngine entityScriptEngine = rootEntity.EntityManager.GetScriptEngine();
            var prefabDataModelBuilderViewModel = new PrefabDataModelBuilderViewModel(prefabToolBoxItem, codeGenerator,
                this.prefabFileSystem, entityScriptEngine, new CompositeTypeExtractor(), new ArgumentExtractor());
            this.stagedScreens.Add(prefabDataModelBuilderViewModel);
       
            var references = prefabFileSystem.ReferenceManager.GetCodeEditorReferences();            
            this.codeEditorFactory.Initialize(prefabFileSystem.DataModelDirectory, references);  
            var prefabDataModelEditorViewModel = new PrefabDataModelEditorViewModel(this.prefabToolBoxItem, this.prefabFileSystem, this.codeEditorFactory);
            this.stagedScreens.Add(prefabDataModelEditorViewModel);
         
            var prefabScriptImporterViewModel = new PrefabScriptsImporterViewModel(prefabToolBoxItem, rootEntity, 
                new ScriptExtractor(new ArgumentExtractor()), new ArgumentExtractor(), prefabFileSystem, scriptEngineFactory);
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
            serializer.Serialize<PrefabProject>(prefabFileSystem.PrefabDescriptionFile, prefabToolBoxItem);
            serializer.Serialize<Entity>(prefabFileSystem.PrefabFile, prefabToolBoxItem.PrefabRoot as Entity);
            UpdateAssemblyReferenceAndNameSpace(prefabToolBoxItem, prefabFileSystem);
            UpdateControlReferencesFile(prefabToolBoxItem.PrefabRoot as Entity);
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
            //In absence of a custom data model, there is no need to update anything
            if (prefabProject.PrefabRoot.EntityManager.Arguments.GetType() == typeof(EmptyModel))
            {
                return;
            }
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

        /// <summary>
        /// Prefabs can use controls in the automation. We need to create a ControlReferences file to include details of these controls.
        /// By default, we create the references with the most recent version of the control irrespective of what version was it created with.
        /// When a prefab is used in a automation project, control references are merged back in automation project control references.
        /// However, the automation project control references will always decide what version of control is used.
        /// </summary>
        /// <param name="prefabRoot"></param>
        private void UpdateControlReferencesFile(Entity prefabRoot)
        {
            var controlReferences = new ControlReferences();
            var referencedControls = prefabRoot.GetComponentsOfType<ControlEntity>(Core.Enums.SearchScope.Descendants);
            foreach(var control in referencedControls)
            {
                var mostRecentVersionOfControl = this.applicationDataManager.GetControlsById(control.ApplicationId, control.ControlId).OrderBy(a => a.Version).Last();
                controlReferences.AddControlReference(new ControlReference(mostRecentVersionOfControl.ApplicationId, mostRecentVersionOfControl.ControlId, mostRecentVersionOfControl.Version));
            }
            this.prefabFileSystem.SaveToFile<ControlReferences>(controlReferences, Path.GetDirectoryName(this.prefabFileSystem.ControlReferencesFile),
                Path.GetFileName(this.prefabFileSystem.ControlReferencesFile));
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
