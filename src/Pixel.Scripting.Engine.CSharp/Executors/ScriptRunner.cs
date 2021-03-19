using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Pixel.Automation.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Scripting.Engine.CSharp
{
    internal class ScriptRunner : ScriptExecutor
    {
        public override async Task<ScriptResult> ExecuteScriptAsync(string scriptCode, ScriptOptions scriptOptions, object globals, object previousState = null)
        {
            ScriptState<object> state = default;

            if (previousState != null)
            {
                state = previousState as ScriptState<object>;
                state = await state.ContinueWithAsync<object>(scriptCode, options: scriptOptions);
            }
            else
            {
                if (globals != null)
                {
                    state = await CSharpScript.RunAsync<object>(scriptCode, options: scriptOptions, globals: globals,
                    globalsType: globals.GetType());
                }
                else
                {
                    state = await CSharpScript.RunAsync<object>(scriptCode, options: scriptOptions);
                }

            }
            ScriptResult scriptResult = new ScriptResult(state.ReturnValue, state);
            return scriptResult;

        }
    }
}
