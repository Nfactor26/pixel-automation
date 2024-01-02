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
        /// Set the globals object for the ScriptEngine.      
        /// </summary>      
        /// <param name="dataModel"></param>
        void SetGlobals(object globals);
       
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

        Task<T> CreateDelegateFromScriptAsync<T>(string scriptCode);

        void ClearState();

        bool HasScriptVariable(string variableName);

        void SetVariableValue<T>(string variableName, T value);

        T GetVariableValue<T>(string variableName);
    }
}
