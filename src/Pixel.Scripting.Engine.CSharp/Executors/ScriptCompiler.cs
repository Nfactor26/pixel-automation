using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
using Pixel.Automation.Core.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Pixel.Scripting.Engine.CSharp
{
    internal class ScriptCompiler : ScriptExecutor
    {
        private const string compiledScriptClass = "Submission#0";
        private const string compiledScriptMethod = "<Factory>";

        public override async Task<ScriptResult> ExecuteScriptAsync(string scriptCode, ScriptOptions scriptOptions, object globals, object previousState = null)
        {

            ScriptState<object> state = default;

            if (previousState != null)
            {
                throw new NotSupportedException("ScriptCompiler can't continue from previous state");
            }

            //TODO : Why are we running script and the compiling and executing again?
            if (globals != null)
            {
                state = await CSharpScript.RunAsync<object>(scriptCode, options: scriptOptions, globals: globals,
                globalsType: globals.GetType());
            }
            else
            {
                state = await CSharpScript.RunAsync<object>(scriptCode, options: scriptOptions);
            }


            var result = CompileAndExecute(state, globals);
            return new ScriptResult(result.ReturnValue, state);
        }

        private ScriptResult CompileAndExecute(ScriptState currentState, Object globals)
        {
            var compilation = currentState.Script.GetCompilation();
            compilation = compilation.WithOptions(compilation.Options.WithOutputKind(OutputKind.DynamicallyLinkedLibrary).WithOptimizationLevel(OptimizationLevel.Debug));

            using (var exeStream = new MemoryStream())
            using (var pdbStream = new MemoryStream())
            {
                var result = compilation.Emit(exeStream, pdbStream: pdbStream, options: new EmitOptions().
                    WithDebugInformationFormat(DebugInformationFormat.Pdb));

                if (result.Success)
                {
                    var assembly = LoadAssembly(exeStream.ToArray(), pdbStream.ToArray());
                    return InvokeEntryPointMethod(globals, assembly);
                }

                var errors = string.Join(Environment.NewLine, result.Diagnostics.Select(x => x.ToString()));
                throw new Exception(errors);
            }

        }


        private ScriptResult InvokeEntryPointMethod(object globals, Assembly assembly)
        {
            // the following line can throw NullReferenceException, if that happens it's useful to notify that an error ocurred
            var type = assembly.GetType(compiledScriptClass);
            var method = type.GetMethod(compiledScriptMethod, BindingFlags.Static | BindingFlags.Public);

            var submissionStates = new object[2];
            submissionStates[0] = globals;
            var result = method.Invoke(null, new[] { submissionStates }) as Task<object>;
            return new ScriptResult(returnValue: result.GetAwaiter().GetResult());

        }


        private Assembly LoadAssembly(byte[] exeBytes, byte[] pdbBytes)
        {
            // this is required for debugging. otherwise, the .dll is not related to the .pdb
            // there might be ways of doing this without "loading", haven't found one yet
            return AppDomain.CurrentDomain.Load(exeBytes, pdbBytes);
        }
    }
}
