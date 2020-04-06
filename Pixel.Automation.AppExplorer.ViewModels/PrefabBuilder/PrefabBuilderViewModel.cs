using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Interfaces.Scripting;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pixel.Automation.Editor.Core.Helpers;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder
{
    public class PrefabBuilderViewModel : Wizard
    {
        private readonly IPrefabFileSystem prefabFileSystem;
        private readonly ICodeGenerator codeGenerator;
        private readonly ICodeEditorFactory codeEditorFactory;
        private readonly IScriptEngineFactory scriptEngineFactory;
        private readonly ISerializer serializer;  

        private PrefabDescription prefabToolBoxItem;

        public PrefabBuilderViewModel(ISerializer serializer, IPrefabFileSystem prefabFileSystem, ICodeGenerator codeGenerator, ICodeEditorFactory codeEditorFactory, IScriptEngineFactory scriptEngineFactory)
        {
            this.DisplayName = "Prefab Builder";
            this.prefabFileSystem = prefabFileSystem;
            this.codeGenerator = codeGenerator;
            this.codeEditorFactory = codeEditorFactory;
            this.scriptEngineFactory = scriptEngineFactory;
            this.serializer = serializer;
        }

        public void Initialize(ApplicationDescription applicationItem, Entity rootEntity)
        {
            this.stagedScreens.Clear();
            PrefabVersion prefabVersion = new PrefabVersion(new Version(1, 0, 0, 0));
            prefabToolBoxItem = new PrefabDescription()
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

            IScriptEngine entityScriptEngine = rootEntity.EntityManager.GetServiceOfType<IScriptEngine>();
            var prefabDataModelBuilderViewModel = new PrefabDataModelBuilderViewModel(prefabToolBoxItem, codeGenerator,
                this.prefabFileSystem, entityScriptEngine, new CompositeTypeExtractor(), new ArgumentExtractor());
            this.stagedScreens.Add(prefabDataModelBuilderViewModel);
       
            var references = new AssemblyReferences().GetReferencesOrDefault();            
            this.codeEditorFactory.Initialize(prefabFileSystem.DataModelDirectory, references.ToArray());  
            var prefabDataModelEditorViewModel = new PrefabDataModelEditorViewModel(this.prefabToolBoxItem, this.prefabFileSystem, this.codeEditorFactory);
            this.stagedScreens.Add(prefabDataModelEditorViewModel);

            var scriptEngine = scriptEngineFactory.CreateScriptEngine(false);
            scriptEngine.SetWorkingDirectory(prefabFileSystem.ScriptsDirectory);
            var prefabScriptImporterViewModel = new PrefabScriptsImporterViewModel(prefabToolBoxItem, rootEntity, 
                new ScriptExtractor(new ArgumentExtractor()), prefabFileSystem, scriptEngine);
            this.stagedScreens.Add(prefabScriptImporterViewModel);

            prefabToolBoxViewModel.NextScreen = prefabDataModelBuilderViewModel;
            prefabDataModelBuilderViewModel.PreviousScreen = prefabToolBoxViewModel;
            prefabDataModelBuilderViewModel.NextScreen = prefabDataModelEditorViewModel;
            prefabDataModelEditorViewModel.PreviousScreen = prefabDataModelBuilderViewModel;
            prefabDataModelEditorViewModel.NextScreen = prefabScriptImporterViewModel;
            prefabScriptImporterViewModel.PreviousScreen = prefabDataModelEditorViewModel;

        }

        public PrefabDescription SavePrefab()
        {
            serializer.Serialize<PrefabDescription>(prefabFileSystem.PrefabDescriptionFile, prefabToolBoxItem);
            serializer.Serialize<Entity>(prefabFileSystem.PrefabFile, prefabToolBoxItem.PrefabRoot as Entity);        
            return this.prefabToolBoxItem;
        }        
       
        public override async void Cancel()
        {
            string prefabsDirectory = prefabFileSystem.WorkingDirectory;
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
            await this.TryCloseAsync(false);
        }

    }
}
