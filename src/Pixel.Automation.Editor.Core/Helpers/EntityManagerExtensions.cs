using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pixel.Automation.Editor.Core.Helpers
{
    public static class DesignTimeEntityManagerExtensions
    {
        /// <summary>
        /// For each scriptable field in component, generate a cache key in the form component.Id-scriptFileName.
        /// If file is empty, no key is returned for the field.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetScriptCacheKeys(this IComponent component)
        {
            ScriptableAttribute scriptableAttribute = component.GetType().GetCustomAttributes(true).OfType<ScriptableAttribute>().FirstOrDefault();
            if (scriptableAttribute != null)
            {
                foreach (var scriptFileProperty in scriptableAttribute.ScriptFiles)
                {
                    var property = component.GetType().GetProperty(scriptFileProperty);
                    string scriptFile = property.GetValue(component)?.ToString();
                    if (!string.IsNullOrEmpty(scriptFile))
                    {
                        yield return $"{component.Id}-{Path.GetFileNameWithoutExtension(scriptFile)}";
                    }
                }
            }
            yield break;
        }

        /// <summary>
        /// Dispose any inline script editors created by component or it's descendant components at design time
        /// </summary>
        /// <param name="component"></param>
        public static void DisposeEditors(this IComponent component)
        {
            List<IComponent> scriptables = new List<IComponent>();
            if (component.GetType().GetCustomAttributes(true).Any(a => a is ScriptableAttribute))
            {
                scriptables.Add(component);
            }
            if (component is Entity entity)
            {
                scriptables.AddRange(entity.GetComponentsWithAttribute<ScriptableAttribute>(SearchScope.Descendants));
            }

            //Dispose any inline script editor that might have been created by this component or any other descendant component.
            //Not all scriptable fields for a component have inline script editor. ScriptEditorFactory will ignore keys that are not present in it's cache.
            var scriptEditorFactory = component.EntityManager.GetServiceOfType<IScriptEditorFactory>();
            foreach (var scriptable in scriptables)
            {
                foreach (var cacheKey in scriptable.GetScriptCacheKeys())
                {
                    scriptEditorFactory.RemoveInlineScriptEditor(cacheKey);
                }
            }
        }

    }
}