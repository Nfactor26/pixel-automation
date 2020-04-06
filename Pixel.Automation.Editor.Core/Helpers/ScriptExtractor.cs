using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
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

            var scriptedActors = entity.GetComponentsWithAttribute<ScriptableAttribute>(SearchScope.Descendants);
            foreach (var scriptedActor in scriptedActors)
            {
                ScriptableAttribute scriptableAttribute = scriptedActor.GetType().GetCustomAttributes(true).OfType<ScriptableAttribute>().FirstOrDefault();
                foreach (var scriptFileProperty in scriptableAttribute.ScriptFiles)
                {
                    var property = scriptedActor.GetType().GetProperty(scriptFileProperty);
                    string scriptFile = property.GetValue(scriptedActor)?.ToString();
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
