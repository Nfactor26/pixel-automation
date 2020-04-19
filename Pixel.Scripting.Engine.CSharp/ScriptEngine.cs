using Dawn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
using Pixel.Scripting.Common.CSharp;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Pixel.Scripting.Engine.CSharp
{
    public class ScriptEngine : IScriptEngine
    {
        #region data members      

        private readonly string cacheFolder = "cache";

        private string scriptsDirectory;

        private ScriptOptions scriptOptions = ScriptOptions.Default;

        private ScriptMetadataResolver scriptMetaDataResolver;

        private MetadataReferenceResolver metaDataReferenceResolver;

        private string baseDirectory;

        private List<string> searchPaths  = new List<string>();

        private List<string> namespaces = new List<string>();
       
        private ScriptExecutor scriptExecutor = default;

        private object scriptGlobals = default;

        #endregion data members

        #region constructor       

        internal ScriptEngine(ScriptExecutor scriptExecutor)
        {
            this.scriptExecutor = scriptExecutor;
        }

        private MetadataReferenceResolver CreateScriptMetaDataResolver()
        {
            scriptMetaDataResolver = ScriptMetadataResolver.Default;
            scriptMetaDataResolver = scriptMetaDataResolver.WithBaseDirectory(baseDirectory);
            scriptMetaDataResolver = scriptMetaDataResolver.WithSearchPaths(this.searchPaths);
            return new CachedScriptMetadataResolver(scriptMetaDataResolver, useCache: true);
        }

        #endregion constructor    

        public string GetWorkingDirectory()
        {
            return this.scriptsDirectory;
        }

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
                Directory.CreateDirectory(this.scriptsDirectory);
                Directory.CreateDirectory(Path.Combine(this.scriptsDirectory, this.cacheFolder));
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


        public IScriptEngine WithSearchPaths(string baseDirectory, params string[] searchPaths)
        {
            Guard.Argument(baseDirectory).NotNull().NotEmpty();

            this.baseDirectory = baseDirectory;
            this.searchPaths = this.searchPaths.Union(searchPaths).ToList<string>();

            metaDataReferenceResolver = CreateScriptMetaDataResolver();
            scriptOptions = scriptOptions.WithMetadataResolver(metaDataReferenceResolver);
            scriptOptions = scriptOptions.AddReferences(ProjectReferences.DesktopDefault.GetReferences());
            scriptOptions = scriptOptions.WithImports(ProjectReferences.NamespaceDefault.Imports);
            scriptOptions = scriptOptions.WithFileEncoding(System.Text.Encoding.UTF8);
            scriptOptions = scriptOptions.WithEmitDebugInformation(true);

            //scriptMetaDataResolver.WithBaseDirectory(baseDirectory);
            //scriptMetaDataResolver = scriptMetaDataResolver.WithSearchPaths(this.searchPaths);

            //for #load references look in to the same folder
            scriptOptions = scriptOptions.WithSourceResolver(new SourceFileResolver(searchPaths,
            baseDirectory));

            //scriptOptions = scriptOptions.WithMetadataResolver(this.scriptMetaDataResolver);
            return this;
        }

        public IScriptEngine WithAdditionalSearchPaths(params string[] searchPaths)
        {
            if (string.IsNullOrEmpty(this.baseDirectory))
                throw new InvalidOperationException($"BaseDirectory is not yet initialized. {nameof(WithSearchPaths)} must be called atleast once before {nameof(WithAdditionalSearchPaths)} can be called");        
        
            this.searchPaths = this.searchPaths.Union(searchPaths).ToList<string>();          
            scriptMetaDataResolver = scriptMetaDataResolver.WithSearchPaths(this.searchPaths);

            //for #load references look in to the same folder
            scriptOptions = scriptOptions.WithSourceResolver(new SourceFileResolver(searchPaths,
            baseDirectory));
          
            return this;
        }

        public void RemoveSearchPaths(params string[] searchPaths)
        {
            this.searchPaths = this.searchPaths.Except(searchPaths).ToList<string>();
            scriptMetaDataResolver = scriptMetaDataResolver.WithSearchPaths(this.searchPaths);
            //scriptOptions = scriptOptions.WithMetadataResolver(this.scriptMetaDataResolver);
        }

        public IScriptEngine WithAdditionalNamespaces(params string[] namespaces)
        {
            this.namespaces = this.namespaces.Union(namespaces).ToList<string>();
            this.scriptOptions = this.scriptOptions.AddImports(this.namespaces);
            return this;
        }

        public void RemoveNamespaces(params string[] namespaces)
        {
            this.namespaces = this.namespaces.Except(namespaces).ToList<string>();
            scriptOptions = scriptOptions.WithImports(this.namespaces);
        }

        public IScriptEngine WithAdditionalAssemblyReferences(params Assembly[] references)
        {           
            foreach (var assembly in references)
            {              
                if (scriptOptions.MetadataReferences.Any(m => m.Display.Equals(assembly.Location)))
                {
                    continue;
                }
                scriptOptions = scriptOptions.AddReferences(assembly);
            }     
            return this;
        }

        public IScriptEngine WithAdditionalAssemblyReferences(string[] assemblyReferences)
        {
            Guard.Argument(assemblyReferences).NotNull();
          
            foreach (var reference in assemblyReferences)
            {
                Assembly assembly = default;              
                if (!Path.IsPathRooted(reference))
                {
                    string assemblyLocation = Path.Combine(Environment.CurrentDirectory, reference);
                    if (!File.Exists(assemblyLocation))
                    {
                        throw new FileNotFoundException($"{assemblyLocation} was not found");
                    }
                    assembly = Assembly.LoadFrom(assemblyLocation);
                }
                else
                {                
                    assembly = Assembly.LoadFrom(reference);
                }  
                
                if(scriptOptions.MetadataReferences.Any(m => m.Display.Equals(assembly.Location)))
                {
                    continue;
                }

                scriptOptions = scriptOptions.AddReferences(assembly);

            }           
          
            return this;

        }

        public void RemoveReferences(params Assembly[] references)
        {
            var currentReferences = this.scriptOptions.MetadataReferences;
            List<MetadataReference> referencesToKeep = new List<MetadataReference>();
            foreach(var metaDataReference in currentReferences)
            {
                if(references.Any(a => a.Location.Equals(metaDataReference.Display)))
                {
                    continue;
                }
                referencesToKeep.Add(metaDataReference);
            }          
            scriptOptions = scriptOptions.WithReferences(new Assembly[] { });
            scriptOptions = scriptOptions.AddReferences(referencesToKeep);
        }

        public bool IsCompleteSubmission(string code)
        {
            var options = new CSharpParseOptions(LanguageVersion.CSharp7, DocumentationMode.Diagnose, SourceCodeKind.Script);
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(code, options: options);
            return SyntaxFactory.IsCompleteSubmission(syntaxTree);
        }

        public (bool,string) IsScriptValid(string code, object globals)
        {
            var script = CSharpScript.Create(code, scriptOptions, globalsType: globals.GetType());
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
                if (scriptVariable.Type.IsAssignableFrom(typeof(T)))
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
            lastExecutionResult = await this.scriptExecutor.ExecuteFileAsync(Path.Combine(this.scriptsDirectory,scriptFile),this.scriptOptions, this.scriptGlobals, lastExecutionResult?.CurrentState);
            return lastExecutionResult;
        }

        public async Task<ScriptResult> ExecuteScriptAsync(string scriptCode)
        {           
            lastExecutionResult = await this.scriptExecutor.ExecuteScriptAsync(scriptCode, this.scriptOptions, this.scriptGlobals, lastExecutionResult?.CurrentState);
            return lastExecutionResult;
        }

        public async Task<T> CreateDelegateAsync<T>(string scriptFile)
        {
            //var state = CSharpScript.Run("int Times(int x) { return x * x; }");
            //var fn = state.CreateDelegate<Func<int, int>>("Times");
            //var result = fn(5);
            //Assert.Equal(25, result);

            //throw new NotImplementedException("Roslyn has an open issue : https://github.com/dotnet/roslyn/issues/3720");

            var scriptLocation = Path.Combine(this.scriptsDirectory, scriptFile);
            if(File.Exists(scriptLocation))
            {
                var scriptCode = File.ReadAllText(scriptLocation);
                lastExecutionResult = await this.scriptExecutor.ExecuteScriptAsync(scriptCode, this.scriptOptions, this.scriptGlobals, lastExecutionResult?.CurrentState);
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

        //public object GetScriptOptions()
        //{
        //    return this.scriptOptions;
        //}

    }
}
