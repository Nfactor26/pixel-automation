using Pixel.Automation.Core.Models;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Pixel.Automation.Core
{
    public interface IScriptEngine
    {         
        /// <summary>
        /// Set the scripts directory folder
        /// </summary>
        /// <param name="scriptsDirectory"></param>
        void SetWorkingDirectory(string scriptsDirectory);

        /// <summary>
        /// Get the script directory folder
        /// </summary>
        /// <returns></returns>
        string GetWorkingDirectory();

        /// <summary>
        /// Set the globals object for the ScriptEngine.      
        /// </summary>      
        /// <param name="dataModel"></param>
        void SetGlobals(object globals);

        /// <summary>
        /// Add directories relative to base directory where dll's will be looked up for reference resolving
        /// </summary>     
        /// <returns></returns>
        IScriptEngine WithSearchPaths(string baseDirectory, params string[] searchPaths);

        /// <summary>
        /// Add additional search paths relative to base directory where dll's will be looked up for reference resolving
        /// </summary>
        /// <param name="searchPaths"></param>
        /// <returns></returns>
        IScriptEngine WithAdditionalSearchPaths(params string[] searchPaths);

        /// <summary>
        /// Remove directories where dll's will be looked up for reference resolving
        /// </summary>
        /// <param name="searchPaths"></param>
        void RemoveSearchPaths(params string[] searchPaths);

        /// <summary>
        /// Add one or more default namespace imports to be made available to the script
        /// </summary>
        /// <param name="namespaces"></param>
        IScriptEngine WithAdditionalNamespaces(params string[] namespaces);

        /// <summary>
        /// Remove one or more default namespace imports from the script
        /// </summary>
        /// <param name="namespaces"></param>
        void RemoveNamespaces(params string[] namespaces);

        /// <summary>
        /// Add one or more default assembly references to be made available to the script
        /// </summary>
        /// <param name="references"></param>
        IScriptEngine WithAdditionalAssemblyReferences(params Assembly[] references);

        /// <summary>
        /// Add one or more default assembly references to be made available to the script.
        /// Assembly location must be a rooted path or relative to working directory.
        /// </summary>
        /// <param name="assemblyReferences"></param>
        IScriptEngine WithAdditionalAssemblyReferences(string[] assemblyReferences);


        /// <summary>
        /// Remove one or more default assembly references to be made available to the script
        /// </summary>
        /// <param name="references"></param>
        void RemoveReferences(params Assembly[] references);

        bool IsCompleteSubmission(string code);

        (bool, string) IsScriptValid(string code, object globals);


        /// <summary>
        /// Get all the varaibles available from the current state of script engine
        /// </summary>
        /// <returns></returns>
        IEnumerable<PropertyDescription> GetScriptVariables();

        Task<ScriptResult> ExecuteFileAsync(string scriptFile);

        Task<ScriptResult> ExecuteScriptAsync(string scriptCode);

        Task<T> CreateDelegateAsync<T>(string scriptFile);


        void ClearState();

        bool HasScriptVariable(string variableName);

        void SetVariableValue<T>(string variableName, T value);

        T GetVariableValue<T>(string variableName);
    }
}
