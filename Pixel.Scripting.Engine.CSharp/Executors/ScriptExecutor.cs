using Microsoft.CodeAnalysis.Scripting;
using Pixel.Automation.Core.Models;
using System.IO;
using System.Threading.Tasks;

namespace Pixel.Scripting.Engine.CSharp
{
    internal abstract class ScriptExecutor
    {
        public async Task<ScriptResult> ExecuteFileAsync(string filePath, ScriptOptions scriptOptions, object globals, object previousState)
        {
            if (File.Exists(filePath))
            {
                scriptOptions = scriptOptions.WithFilePath(filePath); // This is immutable.
                string scriptContent = File.ReadAllText(filePath);
                //scriptContent = $"#line 1 \"{absFilePath}\"{Environment.NewLine}{scriptContent}";
                var result = await ExecuteScriptAsync(scriptContent, scriptOptions, globals, previousState);
                return result;
            }
            throw new FileNotFoundException($"Script file : {filePath} was not found");

        }

        public abstract Task<ScriptResult> ExecuteScriptAsync(string scriptCode, ScriptOptions scriptOptions, object globals, object previousState = null);
       
    }
}
