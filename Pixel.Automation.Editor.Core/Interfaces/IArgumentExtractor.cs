using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using System.Collections.Generic;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IArgumentExtractor
    {
        /// <summary>
        /// Extract Arguments used inside Entity including any nested components used by Entity.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Argument> ExtractArguments(Entity entity);

        /// <summary>
        /// Extract Arguments used by a single component
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        IEnumerable<Argument> ExtractArguments(IComponent component);
    }

}
