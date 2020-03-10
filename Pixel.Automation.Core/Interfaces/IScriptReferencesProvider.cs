using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IScriptReferencesProvider
    {
        /// <summary>
        /// Get a list of assmblies that will be made available to the scripting engine
        /// </summary>
        /// <returns></returns>
        List<Assembly> GetAssembliesToReference();
      
    }
}
