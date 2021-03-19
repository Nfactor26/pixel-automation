using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Core.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Automation.Editor.Core.Helpers
{
    public class ScriptExtractor : IScriptExtactor
    {
        private readonly IArgumentExtractor argumentExtractor;

        public ScriptExtractor(IArgumentExtractor argumentExtractor)
        {
            Guard.Argument(argumentExtractor).NotNull();

            this.argumentExtractor = argumentExtractor;
        }

        public IEnumerable<ScriptStatus> ExtractScripts(Entity entity)
        {
            var arguments = this.argumentExtractor.ExtractArguments(entity);
            foreach (var argument in arguments)
            {
                if (argument.Mode == ArgumentMode.Scripted)
                {
                    if (!string.IsNullOrEmpty(argument.ScriptFile))
                    {
                        yield return new ScriptStatus() { ScriptName = argument.ScriptFile };
                    }
                }
            }

            List<IComponent> scriptedComponents = new List<IComponent>();
            if(entity.GetType().GetCustomAttributes(typeof(ScriptableAttribute), true).Any())
            {
                scriptedComponents.Add(entity);
            }

            var  allComponents  = scriptedComponents.Union(entity.GetComponentsWithAttribute<ScriptableAttribute>(SearchScope.Descendants));
            foreach (var component in allComponents)
            {
                ScriptableAttribute scriptableAttribute = component.GetType().GetCustomAttributes(true).OfType<ScriptableAttribute>().FirstOrDefault();
                if(scriptableAttribute != null)
                {
                    foreach (var scriptFileProperty in scriptableAttribute.ScriptFiles)
                    {
                        var property = component.GetType().GetProperty(scriptFileProperty);
                        string scriptFile = property.GetValue(component)?.ToString();
                        if (!string.IsNullOrEmpty(scriptFile))
                        {
                            yield return new ScriptStatus() { ScriptName = scriptFile };
                        }
                    }
                }                
            }

            yield break;
        }
      
        public IEnumerable<ScriptStatus> ExtractScripts(IComponent component)
        {
            var arguments = this.argumentExtractor.ExtractArguments(component);
            foreach (var argument in arguments)
            {
                if (argument.Mode == ArgumentMode.Scripted)
                {
                    if (!string.IsNullOrEmpty(argument.ScriptFile))
                    {
                        yield return new ScriptStatus() { ScriptName = argument.ScriptFile };
                    }
                }
            }

            ScriptableAttribute scriptableAttribute = component.GetType().GetCustomAttributes(true).OfType<ScriptableAttribute>().FirstOrDefault();
            if (scriptableAttribute != null)
            {
                foreach (var scriptFileProperty in scriptableAttribute.ScriptFiles)
                {
                    var property = component.GetType().GetProperty(scriptFileProperty);
                    string scriptFile = property.GetValue(component)?.ToString();
                    if (!string.IsNullOrEmpty(scriptFile))
                    {
                        yield return new ScriptStatus() { ScriptName = scriptFile };
                    }
                }
            }

            yield break;
        }
    }
}
