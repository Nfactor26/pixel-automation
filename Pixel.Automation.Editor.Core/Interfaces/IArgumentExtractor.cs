using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
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
    }

}
