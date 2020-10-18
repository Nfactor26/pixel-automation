using Dawn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Scripting.Engine.CSharp
{
    public class ScriptEngine : IScriptEngine
    {
        #region data members      
      
        private string scriptsDirectory;

        private Func<ScriptOptions> scriptOptionsGetter;

        private ScriptExecutor scriptExecutor;

        private object scriptGlobals = default;

        ScriptOptions ScriptOptions
        {
            get => scriptOptionsGetter.Invoke();
        }

        #endregion data members

        #region constructor       

        internal ScriptEngine(Func<ScriptOptions> scriptOptionsGetter, ScriptExecutor scriptExecutor)
        {
            this.scriptOptionsGetter = Guard.Argument(scriptOptionsGetter).NotNull();
            this.scriptExecutor = Guard.Argument(scriptExecutor).NotNull();
        }
       
        #endregion constructor    

        public void SetWorkingDirectory(string scriptsDirectory)
        {           
            if(Path.IsPathRooted(scriptsDirectory))
            {
                this.scriptsDirectory = scriptsDirectory;
            }
            else
            {
                this.scriptsDirectory = Path.Combine(Environment.CurrentDirectory, scriptsDirectory);
            } 
            
            if(!Directory.Exists(this.scriptsDirectory))
            {
                throw new ArgumentException($"Directory : {this.scriptsDirectory} doesn't exist.");
            }
        }

        public void SetGlobals(object globalsObject)
        {
            if(this.scriptGlobals != globalsObject)
            {
                this.ClearState();
                this.scriptGlobals = globalsObject;
            }
        }    

        public bool IsCompleteSubmission(string code)
        {
            var options = new CSharpParseOptions(LanguageVersion.CSharp7, DocumentationMode.Diagnose, SourceCodeKind.Script);
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(code, options: options);
            return SyntaxFactory.IsCompleteSubmission(syntaxTree);
        }

        public (bool,string) IsScriptValid(string code, object globals)
        {
            var script = CSharpScript.Create(code, ScriptOptions, globalsType: globals.GetType());
            var compilation = script.GetCompilation();
            compilation = compilation.WithOptions(compilation.Options.WithOutputKind(OutputKind.DynamicallyLinkedLibrary).WithOptimizationLevel(OptimizationLevel.Debug));
            using (var exeStream = new MemoryStream())
            using (var pdbStream = new MemoryStream())
            {
                var result = compilation.Emit(exeStream, pdbStream: pdbStream, options: new EmitOptions());

                if (!result.Success)
                {
                    var errors = string.Join(Environment.NewLine, result.Diagnostics.Select(x => x.ToString()));
                    return (false, errors);
                }

            }
          
            return (true, string.Empty);
        }

        public IEnumerable<PropertyDescription> GetScriptVariables()
        {
            var variables = (lastExecutionResult?.CurrentState as ScriptState)?.Variables ?? ImmutableArray.Create<ScriptVariable>();
            foreach(var variable in variables)
            {
                yield return new PropertyDescription(variable.Name, variable.Type);
            }
            yield break;
        }

        public bool HasScriptVariable(string variableName)
        {
            return GetScriptVariables().Any(v => v.PropertyName.Equals(variableName));
        }

        public void SetVariableValue<T>(string variableName, T value)
        {
            var scriptVariable = (lastExecutionResult?.CurrentState as ScriptState).GetVariable(variableName);
            if(scriptVariable != null)
            {
                if (scriptVariable.Type.IsAssignableFrom(value.GetType()))
                {
                    scriptVariable.Value = value;
                    return;
                }
                throw new ArgumentException($"Script variable : {variableName} has type : {scriptVariable.Type}. Value of type : {typeof(T)} can't be assigned");
            }
            throw new ArgumentException($"Script variable : {variableName} isn't defined");
        }

        public T GetVariableValue<T>(string variableName)
        {
            var scriptVariable = (lastExecutionResult?.CurrentState as ScriptState).GetVariable(variableName);
            if (scriptVariable != null)
            {
                if (typeof(T).IsAssignableFrom(scriptVariable.Type))
                {
                    return (T)scriptVariable.Value;
                }
                throw new ArgumentException($"Script variable : {variableName} has type : {scriptVariable.Type}. Value can't be assigned to type {typeof(T)}");
            }
            throw new ArgumentException($"Script variable : {variableName} isn't defined");
        }

        private ScriptResult lastExecutionResult = default;

        public async Task<ScriptResult> ExecuteFileAsync(string scriptFile)
        {         
            lastExecutionResult = await this.scriptExecutor.ExecuteFileAsync(GetScriptLocation(scriptFile), this.ScriptOptions, this.scriptGlobals, lastExecutionResult?.CurrentState);
            return lastExecutionResult;
        }

        public async Task<ScriptResult> ExecuteScriptAsync(string scriptCode)
        {           
            lastExecutionResult = await this.scriptExecutor.ExecuteScriptAsync(scriptCode, this.ScriptOptions, this.scriptGlobals, lastExecutionResult?.CurrentState);
            return lastExecutionResult;
        }

        public async Task<T> CreateDelegateAsync<T>(string scriptFile)
        {
            //var state = CSharpScript.Run("int Times(int x) { return x * x; }");
            //var fn = state.CreateDelegate<Func<int, int>>("Times");
            //var result = fn(5);
            //Assert.Equal(25, result);

            //throw new NotImplementedException("Roslyn has an open issue : https://github.com/dotnet/roslyn/issues/3720");

            var scriptLocation = GetScriptLocation(scriptFile);
            if(File.Exists(scriptLocation))
            {
                var scriptCode = File.ReadAllText(scriptLocation);
                lastExecutionResult = await this.scriptExecutor.ExecuteScriptAsync(scriptCode, this.ScriptOptions, this.scriptGlobals, lastExecutionResult?.CurrentState);
                if (lastExecutionResult.ReturnValue is T del)
                {
                    return del;
                }
                throw new InvalidOperationException($"Script didn't return delegate of {typeof(T)}");
            }

            throw new FileNotFoundException($"Script file : {scriptFile}  doesn't exist");
        }

        public void ClearState()
        {
            lastExecutionResult = null;
        }

        private string GetScriptLocation(string scriptFile)
        {
            string targetFile = string.Empty;
            if (Path.IsPathRooted(scriptFile))
            {
                targetFile = scriptFile;
            }
            else
            {
                targetFile = Path.Combine(this.scriptsDirectory, scriptFile);
            }
            if(File.Exists(targetFile))
            {
                return targetFile;
            }
            throw new FileNotFoundException($"Script File : {scriptFile} could not be located");
        }

    }
}
