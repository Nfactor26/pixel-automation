using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Scripting.Components;
using System;
using System.Collections.Generic;
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

        private List<Argument> arguments = new List<Argument>();

        public BindableCollection<ScriptStatus> RequiredScripts { get; set; } = new BindableCollection<ScriptStatus>();

        public bool HasScripts { get => RequiredScripts.Count() > 0; }

        public PrefabScriptsImporterViewModel(PrefabDescription prefabToolBoxItem, Entity rootEntity, IPrefabFileSystem prefabFileSystem, IScriptEngine scriptEngine)
        {         
            this.prefabToolBoxItem = prefabToolBoxItem;
            this.prefabFileSystem = prefabFileSystem;
            this.scriptEngine = scriptEngine;
            this.rootEntity = rootEntity;
            ExtractArguments(rootEntity);

        }

        private void ExtractArguments(Entity entity)
        {
            var allComponents = entity.GetAllComponents();
            foreach (var component in allComponents)
            {
                if (component is GroupEntity groupEntity && (groupEntity.GroupActor != null))
                {
                    AddArgumentsForComponent(groupEntity.GroupActor);
                    continue;
                }
                AddArgumentsForComponent(component);
            }

            void AddArgumentsForComponent(IComponent component)
            {
                foreach (var argument in GetArguments(component))
                {
                    if (argument != null)
                    {
                        arguments.Add(argument);
                    }
                }
            }

            IEnumerable<Argument> GetArguments(IComponent component)
            {
                var componentProperties = component.GetType().GetProperties();
                foreach (var property in componentProperties)
                {
                    if (property.PropertyType.Equals(typeof(Argument)) || property.PropertyType.IsSubclassOf(typeof(Argument)))
                    {
                        yield return property.GetValue(component) as Argument;
                    }
                }
            }
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

        private void FindRequiredScripts()
        {
            foreach (var argument in arguments)
            {
                if (argument.Mode == ArgumentMode.Scripted)
                {
                    if (!string.IsNullOrEmpty(argument.ScriptFile))
                    {
                        RequiredScripts.Add(new ScriptStatus() { ScriptName = argument.ScriptFile });
                    }
                }
            }
          
            var scriptedActors = this.rootEntity.GetComponentsWithAttribute<ScriptableAttribute>(Core.Enums.SearchScope.Descendants);
            foreach (var scriptedActor in scriptedActors)
            {
                ScriptableAttribute scriptableAttribute = scriptedActor.GetType().GetCustomAttributes(true).OfType<ScriptableAttribute>().FirstOrDefault();
                foreach(var scriptFileProperty in scriptableAttribute.ScriptFiles)
                {
                    var property = scriptedActor.GetType().GetProperty(scriptFileProperty);
                    string scriptFile = property.GetValue(scriptedActor)?.ToString();
                    if (!string.IsNullOrEmpty(scriptFile))
                    {
                        RequiredScripts.Add(new ScriptStatus() { ScriptName = scriptFile });
                    }
                }

               
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

            if(RequiredScripts.Any(a => !a.IsValid))
            {
                AddOrAppendErrors("", "Some of the scripts could not be compiled.");
            }
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
            FindRequiredScripts();
            ValidateScripts();
            await base.OnActivateAsync(cancellationToken);
        }

    }
}
