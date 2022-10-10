using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Core.ViewModels;
using Pixel.Scripting.Reference.Manager.Contracts;
using Serilog;
using System.IO;
using System.Reflection;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder
{
    /// <summary>
    /// Show the scripts that needs to be imported and validate them by trying to compile required scripts.
    /// If all the scripts are valid, copy the scripts from process directory to prefab directory.
    /// </summary>
    public class PrefabScriptsImporterViewModel : StagedSmartScreen
    {
        private readonly ILogger logger = Log.ForContext<PrefabScriptsImporterViewModel>();

        private readonly PrefabProject prefabProject;
        private readonly Entity rootEntity;

        private readonly IScriptEngineFactory scriptEngineFactory;
        private readonly IPrefabFileSystem prefabFileSystem;
        private readonly IScriptExtactor scriptExtractor;
        private readonly IArgumentExtractor argumentExtractor;
        private readonly IReferenceManager referenceManager;


        public BindableCollection<ScriptStatus> RequiredScripts { get; set; } = new BindableCollection<ScriptStatus>();

        public bool HasScripts { get => RequiredScripts.Count() > 0; }

        public PrefabScriptsImporterViewModel(PrefabProject prefabToolBoxItem, Entity rootEntity, IScriptExtactor scriptExtractor,
            IArgumentExtractor argumentExtractor, IPrefabFileSystem prefabFileSystem, IScriptEngineFactory scriptEngineFactory, IReferenceManager referenceManager)
        {
            this.prefabProject = Guard.Argument(prefabToolBoxItem).NotNull().Value;
            this.prefabFileSystem = Guard.Argument(prefabFileSystem).NotNull().Value;
            this.scriptExtractor = Guard.Argument(scriptExtractor).NotNull().Value;
            this.argumentExtractor = Guard.Argument(argumentExtractor).NotNull().Value;
            this.scriptEngineFactory = Guard.Argument(scriptEngineFactory).NotNull().Value;
            this.rootEntity = Guard.Argument(rootEntity).NotNull();
            this.referenceManager = Guard.Argument(referenceManager).NotNull().Value;

        }      

        public override bool TryProcessStage(out string errorDescription)
        {
            try
            {
                UpdateScriptFilePath();
                ClearErrors("");               
                errorDescription = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
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
            logger.Information($"Activate screen is {nameof(PrefabScriptsImporterViewModel)}");

            this.RequiredScripts.AddRange(scriptExtractor.ExtractScripts(rootEntity));
            ValidateScripts();
            await base.OnActivateAsync(cancellationToken);
        }

        /// <summary>
        /// Copy scripts from automation project to prefab project and update the automation project namespace to prefab project namespace
        /// </summary>
        private void CopyScriptsToPrefabsDirectory()
        {
            foreach (var scriptFile in RequiredScripts)
            {
                string filePath = Path.Combine(rootEntity.EntityManager.GetCurrentFileSystem().WorkingDirectory, scriptFile.ScriptName);
                string fileContent = File.ReadAllText(filePath);
                string projectNamespace = this.rootEntity.EntityManager.Arguments.GetType().Namespace;
                fileContent = fileContent.Replace($"using {projectNamespace};", $"using {prefabProject.Namespace};");
                File.WriteAllText(Path.Combine(prefabFileSystem.WorkingDirectory, $"Scripts\\{Path.GetFileName(scriptFile.ScriptName)}"), fileContent);
                logger.Information($"Copied script file : {scriptFile} from {filePath}");
            }
        }

        private void UpdateScriptFilePath()
        {
            var argumentsInUse = this.argumentExtractor.ExtractArguments(this.rootEntity);
            foreach(var argument in argumentsInUse)
            {
                if(argument.Mode == Core.Arguments.ArgumentMode.Scripted)
                {
                    //Make all path relative to Scripts. Assumption here is that even if any shared script file was used in actual process, it would have been referred from 
                    //Scripts folder
                    argument.ScriptFile = $"Scripts\\{Path.GetFileName(argument.ScriptFile)}";                    
                }
            }

            var scriptedActors = this.rootEntity.GetComponentsWithAttribute<ScriptableAttribute>(Core.Enums.SearchScope.Descendants);
            foreach (var scriptedActor in scriptedActors)
            {
                ScriptableAttribute scriptableAttribute = scriptedActor.GetType().GetCustomAttributes(true).OfType<ScriptableAttribute>().FirstOrDefault();
                foreach (var scriptFileProperty in scriptableAttribute.ScriptFiles)
                {
                    var property = scriptedActor.GetType().GetProperty(scriptFileProperty);
                    string scriptFile = property.GetValue(scriptedActor)?.ToString();
                    if (!string.IsNullOrEmpty(scriptFile))
                    {
                        property.SetValue(scriptedActor, $"Scripts\\{Path.GetFileName(scriptFile)}");
                    }
                }
            }
        }

        private void ValidateScripts()
        {
            logger.Information("Validating script files");

            ClearErrors("");
            CopyScriptsToPrefabsDirectory();          

            var dataModelAssembly = this.PreviousScreen.GetProcessedResult() as Assembly;
            var dataModelType = dataModelAssembly.GetTypes().FirstOrDefault(t => t.Name.Equals(Constants.PrefabDataModelName));
            object dataModelInstance = Activator.CreateInstance(dataModelType);

            scriptEngineFactory.WithAdditionalAssemblyReferences(this.referenceManager.GetScriptEngineReferences());
            scriptEngineFactory.WithAdditionalAssemblyReferences(dataModelAssembly);
            var scriptEngine = scriptEngineFactory.CreateScriptEngine(prefabFileSystem.WorkingDirectory);
            scriptEngine.SetWorkingDirectory(prefabFileSystem.WorkingDirectory);
            
            foreach (var requiredScript in RequiredScripts)
            {
                string fileContent = File.ReadAllText(Path.Combine(prefabFileSystem.ScriptsDirectory, requiredScript.ScriptName));
                var validationResult = scriptEngine.IsScriptValid(fileContent, dataModelInstance);
                requiredScript.UpdateStatus(validationResult.Item1, validationResult.Item2);
            }

            if (RequiredScripts.Any(a => !a.IsValid))
            {
                AddOrAppendErrors("", "Some of the scripts could not be compiled.");
                logger.Error("Validating of script files failed");
                return;
            }

            logger.Information("Validating of script files completed");

        }

    }
}
