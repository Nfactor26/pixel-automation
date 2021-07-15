using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IPrefabArgumentMapper
    {
        /// <summary>
        /// Identies the possible mappings that can be done from source properties on assignFrom Type or variables declared in script engine
        /// to target properties in assignTo Type or vice-versa. For each identified mapping, a PropertyMap definition is created.
        /// </summary>
        /// <param name="scriptEngine"></param>
        /// <param name="assignFrom"></param>
        /// <param name="assignTo"></param>
        /// <returns></returns>
        IEnumerable<PropertyMap> GenerateMapping(IScriptEngine scriptEngine, Type assignFrom, Type assignTo);


        /// <summary>
        /// Generate the mapping code for a collection of PropertyMap definitions
        /// </summary>
        /// <param name="mappings"></param>
        /// <returns></returns>
        string GeneratedMappingCode(IEnumerable<PropertyMap> mappings, Type assignFrom, Type assignTo);
       
    }
}
