using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Core.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder
{
    /// <summary>
    /// Show the scripts that needs to be imported and validate them by trying to compile required scripts.
    /// If all the scripts are valid, copy the scripts from process directory to prefab directory.
    /// </summary>
    public class PrefabScriptsImporterViewModel : StagedSmartScreen
    {      
        private readonly PrefabDescription prefabToolBoxItem;
        private readonly Entity rootEntity;

        private readonly IScriptEngine scriptEngine;
        private readonly IPrefabFileSystem prefabFileSystem;
        private readonly IScriptExtactor scriptExtractor;


        public BindableCollection<ScriptStatus> RequiredScripts { get; set; } = new BindableCollection<ScriptStatus>();

        public bool HasScripts { get => RequiredScripts.Count() > 0; }

        public PrefabScriptsImporterViewModel(PrefabDescription prefabToolBoxItem, Entity rootEntity, IScriptExtactor scriptExtractor,
            IPrefabFileSystem prefabFileSystem, IScriptEngine scriptEngine)
        {
            Guard.Argument(prefabToolBoxItem).NotNull();
            Guard.Argument(rootEntity).NotNull();
            Guard.Argument(prefabFileSystem).NotNull();         
            Guard.Argument(scriptExtractor).NotNull();
            Guard.Argument(scriptEngine).NotNull();

            this.prefabToolBoxItem = prefabToolBoxItem;
            this.prefabFileSystem = prefabFileSystem;
            this.scriptExtractor = scriptExtractor;
            this.scriptEngine = scriptEngine;           
            this.rootEntity = rootEntity;           

        }      

        public override bool TryProcessStage(out string errorDescription)
        {
            try
            {
                ClearErrors("");             
                errorDescription = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                errorDescription = ex.Message;
                AddOrAppendErrors("", ex.Message);
                return false;
            }
        }

        public override object GetProcessedResult()
        {
            return string.Empty;
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            this.RequiredScripts.AddRange(scriptExtractor.ExtractScripts(rootEntity));
            ValidateScripts();
            await base.OnActivateAsync(cancellationToken);
        }

        private void CopyScriptsToPrefabsDirectory()
        {
            foreach (var scriptFile in RequiredScripts)
            {
                string filePath = Path.Combine(rootEntity.EntityManager.GetCurrentFileSystem().ScriptsDirectory, scriptFile.ScriptName);
                string fileContent = File.ReadAllText(filePath);
                fileContent = fileContent.Replace("using Pixel.Automation.Project.DataModels;", $"using {prefabToolBoxItem.NameSpace};");
                File.WriteAllText(Path.Combine(prefabFileSystem.ScriptsDirectory, scriptFile.ScriptName), fileContent);
            }
        }

        private void ValidateScripts()
        {
            ClearErrors("");
            CopyScriptsToPrefabsDirectory();

            var dataModelAssembly = (this.PreviousScreen as IStagedScreen).GetProcessedResult() as Assembly;
            var dataModelType = dataModelAssembly.GetTypes().FirstOrDefault(t => t.Name.Equals(Constants.PrefabDataModelName));
            object dataModelInstance = Activator.CreateInstance(dataModelType);

            scriptEngine.WithSearchPaths(Environment.CurrentDirectory, Path.Combine(Environment.CurrentDirectory, ""));
            scriptEngine.WithAdditionalAssemblyReferences(prefabFileSystem.GetAssemblyReferences());
            scriptEngine.WithAdditionalAssemblyReferences(dataModelAssembly);

            foreach (var requiredScript in RequiredScripts)
            {
                string fileContent = File.ReadAllText(Path.Combine(prefabFileSystem.ScriptsDirectory, requiredScript.ScriptName));
                var validationResult = scriptEngine.IsScriptValid(fileContent, dataModelInstance);
                requiredScript.UpdateStatus(validationResult.Item1, validationResult.Item2);
            }

            if (RequiredScripts.Any(a => !a.IsValid))
            {
                AddOrAppendErrors("", "Some of the scripts could not be compiled.");
            }
        }

    }
}
