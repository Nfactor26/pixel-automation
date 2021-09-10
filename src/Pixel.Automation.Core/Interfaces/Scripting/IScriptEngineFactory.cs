using System.Collections.Generic;
using System.Reflection;

namespace Pixel.Automation.Core
{
    public interface IScriptEngineFactory
    {
        /// <summary>
        /// Add directories relative to base directory where #r and #load references can be resolved
        /// </summary>     
        /// <returns></returns>
        IScriptEngineFactory WithSearchPaths(string baseDirectory, params string[] searchPaths);

        /// <summary>
        /// Add additional search paths relative to base directory where where #r and #load references can be resolved
        /// </summary>
        /// <param name="searchPaths"></param>
        /// IScriptEngineFactoryreturns></returns>
        IScriptEngineFactory WithAdditionalSearchPaths(params string[] searchPaths);

        /// <summary>
        /// Remove directories where #r and #load references can be resolved
        /// </summary>
        /// <param name="searchPaths"></param>
        void RemoveSearchPaths(params string[] searchPaths);

        /// <summary>
        /// Add one or more default namespace imports to be made available to the script
        /// </summary>
        /// <param name="namespaces"></param>
        IScriptEngineFactory WithAdditionalNamespaces(params string[] namespaces);

        /// <summary>
        /// Remove one or more default namespace imports from the script
        /// </summary>
        /// <param name="namespaces"></param>
        void RemoveNamespaces(params string[] namespaces);

        /// <summary>
        /// Add one or more default assembly references to be made available to the script
        /// </summary>
        /// <param name="references"></param>
        IScriptEngineFactory WithAdditionalAssemblyReferences(params Assembly[] references);

        /// <summary>
        /// Add one or more default assembly references to be made available to the script.
        /// Assembly location must be a rooted path or relative to working directory.
        /// </summary>
        /// <param name="assemblyReferences"></param>
        IScriptEngineFactory WithAdditionalAssemblyReferences(IEnumerable<string> assemblyReferences);

        /// <summary>
        /// Remove one or more default assembly references to be made available to the script
        /// </summary>
        /// <param name="references"></param>
        void RemoveReferences(params Assembly[] references);

        /// <summary>
        /// Create script engine
        /// </summary>     
        /// <returns></returns>
        IScriptEngine CreateScriptEngine(string workingDirectory);

        /// <summary>
        /// Create  script engine for use with interactive execution
        IScriptEngine CreateInteractiveScriptEngine();
    }
}
