using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core.ViewModels;
using System.Collections.Generic;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IScriptExtactor
    {
        /// <summary>
        /// Find all the script files used by entity and its child components.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        IEnumerable<ScriptStatus> ExtractScripts(Entity entity);

        /// <summary>
        /// Find all the script file used by a single component.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        IEnumerable<ScriptStatus> ExtractScripts(IComponent component);
    }
}
