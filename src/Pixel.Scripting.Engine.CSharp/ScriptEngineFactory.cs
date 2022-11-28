using Dawn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Pixel.Automation.Core;
using Pixel.Automation.Reference.Manager;
using Pixel.Scripting.Common.CSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pixel.Scripting.Engine.CSharp
{
    public class ScriptEngineFactory : IScriptEngineFactory
    {
        #region data members    

        private readonly ILogger logger = Log.ForContext<ScriptEngineFactory>();

        private ScriptOptions scriptOptions = ScriptOptions.Default;

        private CachedScriptMetadataResolver metaDataReferenceResolver;

        private string baseDirectory = Environment.CurrentDirectory;

        private List<string> searchPaths = new List<string>();

        private List<string> namespaces = new List<string>();

        private List<WeakReference<IScriptEngine>> createdScriptEngines = new List<WeakReference<IScriptEngine>>();

        private readonly object locker = new object();      

        #endregion data members

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="applicationSettings"></param>
        public ScriptEngineFactory()
        {          
            this.metaDataReferenceResolver = CreateScriptMetaDataResolver();
            this.scriptOptions = this.scriptOptions.WithMetadataResolver(metaDataReferenceResolver);
            this.scriptOptions = this.scriptOptions.AddReferences(AssemblyReferences.DefaultReferences.GetReferences());
            this.scriptOptions = this.scriptOptions.WithImports(AssemblyReferences.DefaultNamespaces.Imports);
            this.scriptOptions = this.scriptOptions.WithFileEncoding(System.Text.Encoding.UTF8);
            this.scriptOptions = this.scriptOptions.WithEmitDebugInformation(true);
                
            logger.Information($"{nameof(ScriptEngineFactory)} created and initialized.");
        }

        /// <summary>
        /// Configure lookup path for #r directives
        /// searchPaths : An ordered set of fully qualified  paths which are searched when resolving assembly names.
        /// baseDirectory : Directory used when resolving relative paths
        /// </summary>
        /// <returns></returns>
        private CachedScriptMetadataResolver CreateScriptMetaDataResolver()
        {
            var scriptMetaDataResolver = ScriptMetadataResolver.Default;
            scriptMetaDataResolver = scriptMetaDataResolver.WithBaseDirectory(baseDirectory);
            scriptMetaDataResolver = scriptMetaDataResolver.WithSearchPaths(this.searchPaths);
            var resolver = new CachedScriptMetadataResolver(scriptMetaDataResolver, useCache: true);              
            return resolver;
        }

        ///<inheritdoc/>
        public IScriptEngineFactory WithSearchPaths(string baseDirectory, params string[] searchPaths)
        {
            Guard.Argument(baseDirectory).NotNull().NotEmpty();

            lock (locker)
            {
                this.baseDirectory = baseDirectory;
                this.searchPaths = this.searchPaths.Union(searchPaths).ToList<string>();
                UpdateScriptOptions();
                return this;
            }
        }

        ///<inheritdoc/>
        public IScriptEngineFactory WithAdditionalSearchPaths(params string[] searchPaths)
        {
            if (string.IsNullOrEmpty(this.baseDirectory))
            {
                throw new InvalidOperationException($"BaseDirectory is not yet initialized. {nameof(WithSearchPaths)} must be called atleast once before {nameof(WithAdditionalSearchPaths)} can be called");
            }

            lock (locker)
            {
                this.searchPaths = this.searchPaths.Union(searchPaths).ToList<string>();
                UpdateScriptOptions();
                return this;
            }
        }

        ///<inheritdoc/>
        public void RemoveSearchPaths(params string[] searchPaths)
        {
            lock (locker)
            {
                this.searchPaths = this.searchPaths.Except(searchPaths).ToList<string>();
                UpdateScriptOptions();
            }
        }

        ///<inheritdoc/>
        void UpdateScriptOptions()
        {
            var scriptMetaDataResolver = ScriptMetadataResolver.Default;
            scriptMetaDataResolver = scriptMetaDataResolver.WithBaseDirectory(this.baseDirectory);
            scriptMetaDataResolver = scriptMetaDataResolver.WithSearchPaths(this.searchPaths);

            this.metaDataReferenceResolver = this.metaDataReferenceResolver.WithScriptMetaDataResolver(scriptMetaDataResolver);
            this.scriptOptions = this.scriptOptions.WithMetadataResolver(metaDataReferenceResolver);

            //for #load references
            this.scriptOptions = this.scriptOptions.WithSourceResolver(new SourceFileResolver(searchPaths, baseDirectory));
        }

        ///<inheritdoc/>
        public IScriptEngineFactory WithAdditionalNamespaces(params string[] namespaces)
        {
            lock (locker)
            {
                this.namespaces = this.namespaces.Union(namespaces).ToList<string>();
                foreach(var import in namespaces)
                {
                    if(!this.scriptOptions.Imports.Contains(import))
                    {
                        this.scriptOptions = this.scriptOptions.AddImports(import);
                    }
                }                
                return this;
            }
        }

        ///<inheritdoc/>
        public void RemoveNamespaces(params string[] namespaces)
        {
            lock (locker)
            {
                this.namespaces = this.namespaces.Except(namespaces).ToList<string>();
                this.scriptOptions = this.scriptOptions.WithImports(AssemblyReferences.DefaultNamespaces.Imports.Union(this.namespaces));
            }
        }

        ///<inheritdoc/>
        public IScriptEngineFactory WithAdditionalAssemblyReferences(params Assembly[] references)
        {
            lock (locker)
            {
                foreach (var assembly in references)
                {
                    if (this.scriptOptions.MetadataReferences.Any(m => m.Display.Equals(assembly.Location)))
                    {
                        continue;
                    }                   
                    this.scriptOptions = this.scriptOptions.AddReferences(assembly);
                }
                return this;
            }
        }

        ///<inheritdoc/>
        public IScriptEngineFactory WithAdditionalAssemblyReferences(IEnumerable<string> assemblyReferences)
        {
            Guard.Argument(assemblyReferences).NotNull();

            lock (locker)
            {
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

                    if (this.scriptOptions.MetadataReferences.Any(m => m.Display.Equals(assembly.Location)))
                    {
                        continue;
                    }                  
                    this.scriptOptions = this.scriptOptions.AddReferences(assembly);
                }
                return this;
            }
        }

        ///<inheritdoc/>
        public IScriptEngineFactory WithWhiteListedReferences(IEnumerable<string> whiteListedReferences)
        {
            Guard.Argument(whiteListedReferences).NotNull();
            foreach(var reference in whiteListedReferences)
            {
               _ = this.metaDataReferenceResolver.WithWhiteListedReference(reference);
            }
            return this;
        }

        ///<inheritdoc/>
        public void RemoveReferences(params Assembly[] references)
        {
            lock (locker)
            {
                var currentReferences = this.scriptOptions.MetadataReferences;
                List<MetadataReference> referencesToKeep = new List<MetadataReference>();
                foreach (var metaDataReference in currentReferences)
                {
                    if (references.Any(a => a.Location.Equals(metaDataReference.Display)))
                    {
                        continue;
                    }
                    referencesToKeep.Add(metaDataReference);
                }
                this.scriptOptions = this.scriptOptions.WithReferences(new Assembly[] { });
                this.scriptOptions = this.scriptOptions.AddReferences(referencesToKeep);

                List<WeakReference<IScriptEngine>> itemsToRemove = new List<WeakReference<IScriptEngine>>();
                //Clear the state of each script engine since scriptOptions don't match with ScriptState anymore
                foreach (var scriptEngineReference in this.createdScriptEngines)
                {
                    if (scriptEngineReference.TryGetTarget(out IScriptEngine scriptEngine))
                    {
                        scriptEngine.ClearState();
                    }
                    else
                    {
                        itemsToRemove.Add(scriptEngineReference);
                    }
                }

                this.createdScriptEngines.RemoveAll(a=> itemsToRemove.Contains(a));
            }

        }

        ///<inheritdoc/>
        public IScriptEngine CreateScriptEngine(string workingDirectory)
        {
            IScriptEngine scriptEngine = new ScriptEngine(() => this.scriptOptions, new ScriptRunner());
            scriptEngine.SetWorkingDirectory(workingDirectory);
            this.createdScriptEngines.Add(new WeakReference<IScriptEngine>(scriptEngine));
            logger.Information($"Created a new instance of {nameof(ScriptEngine)} with working directory : {workingDirectory}");
            return scriptEngine;
        }

        ///<inheritdoc/>
        public IScriptEngine CreateInteractiveScriptEngine()
        {
            IScriptEngine scriptEngine = new ScriptEngine(() => this.scriptOptions, new ScriptRunner());
            this.createdScriptEngines.Add(new WeakReference<IScriptEngine>(scriptEngine));
            return scriptEngine;
        }
    }
}
