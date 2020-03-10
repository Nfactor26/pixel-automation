using Ninject;
using Ninject.Extensions.ChildKernel;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Designer.ViewModels.Modules;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Threading;

namespace Pixel.Automation.Designer.ViewModels
{
    /// <summary>
    /// Service Resolver wraps ninject child kernel to resolve any services.
    /// Ninject child kernel will delegate any service request it can't handle to parent kernel.
    /// The idea is to isolate all dependencies of a Automation Project in to child kernel so that they can be configured for different lifespan such as singleton when required.
    /// Constants can be registered as default as well so that you get same instance throughout the project.
    /// </summary>
    public class ServiceResolver : IServiceResolver, ICloneable, IDisposable
    {
        private readonly IKernel kernel = default;

        public ServiceResolver(IKernel parentKernel)
        {
            this.kernel = new ChildKernel(parentKernel, new WorkspaceModules(), new ScopedModules(), new ScriptingModule(), new DevicesModule());            
        }        

        /// <summary>
        /// Get service of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key = null)
        {
            var instance = kernel.Get<T>(key);
            if (instance != null)
                return instance;
            throw new Exception(string.Format("Could not locate any instances of contract {0}.", typeof(T).ToString()));
        }


        /// <summary>
        /// Get all services of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetAll<T>()
        {
            var instances = kernel.GetAll<T>();
            if (instances != null)
                return instances;
            throw new Exception(string.Format("Could not locate any instances of contract {0}.", typeof(T).ToString()));
        }

        /// <summary>
        /// Register a instance for a given service type. Whenever this service type is requested, this instance will be returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        public void RegisterDefault<T>(T instance) where T : class               
        {
            kernel.Bind<T>().ToConstant(instance);
        }

        public void ConfigureDefaultServices(IFileSystem fileSystem, object globalsInstance)
        {
            ConfigureScriptEngine(fileSystem, globalsInstance?.GetType().Assembly);
            ConfigureArgumentProcessor(globalsInstance);
            ConfigureScriptEditor(fileSystem, globalsInstance?.GetType());          
        }


        /// <summary>
        /// Configure Script Engine. Script engines will have to be configured for primary as well as secondary entity manager.
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="previousModel"></param>
        /// <param name="newModel"></param>
        private void ConfigureScriptEngine(IFileSystem fileSystem, Assembly globalsAssembly)
        {
            IScriptEngineFactory scriptEngineFactory = Get<IScriptEngineFactory>(); //Transient
            IScriptEngine scriptEngine = scriptEngineFactory.CreateScriptEngine(false);
            scriptEngine.SetWorkingDirectory(fileSystem.ScriptsDirectory);
            scriptEngine.WithSearchPaths(System.Environment.CurrentDirectory, Path.Combine(System.Environment.CurrentDirectory, ""), Path.Combine(System.Environment.CurrentDirectory, "Components"), Path.Combine(System.Environment.CurrentDirectory, "Core"));
            scriptEngine.WithAdditionalAssemblyReferences(fileSystem.GetAssemblyReferences());
            if(globalsAssembly != null)
            {
                scriptEngine.WithAdditionalAssemblyReferences(globalsAssembly);
            }
            this.RegisterDefault<IScriptEngine>(scriptEngine);            
        }

        /// <summary>
        /// Configure Argument processor. Argument Processor uses a script engine. Script engine must be hence configured before arugment 
        /// processor is configured.
        /// </summary>
        /// <param name="registerAsDefault"></param>
        private void ConfigureArgumentProcessor(object globals)
        {
            IArgumentProcessor argumentProcessor = Get<IArgumentProcessor>();
            if(globals != null)
            {
                argumentProcessor.SetGlobals(globals);
            }
            //this.RegisterDefault<IArgumentProcessor>(argumentProcessor);
        }

        /// <summary>
        /// Configure CodeEditor. CodeEditory should be configured only for primary entity manager.
        /// </summary>
        /// <param name="fileSystem"></param>
        //public void ConfigureCodeEditor(IFileSystem fileSystem)
        //{
        //    ICodeEditorFactory codeEditorFactory = Get<ICodeEditorFactory>();
        //    codeEditorFactory.Initialize(fileSystem.DataModelDirectory, fileSystem.GetAssemblyReferences());
        //}

        /// <summary>
        /// Configure ScriptEditor with specified globalsType. This method can be called again with a different globalsType.
        /// On doing so,  underlying workspace will be disposed and new workspace will be created to reflect change in globalsType.
        /// If there are any inline script editor controls, they are not impacted. They will pick up this change in globalsType.
        /// ScriptEditor should be configured for primary as well as secondary entity manager since they have different globalsType.
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="globalsType"></param>
        private void ConfigureScriptEditor(IFileSystem fileSystem, Type globalsType)
        {
            Dispatcher dispatcher = System.Windows.Application.Current.Dispatcher;
            if (!dispatcher.CheckAccess())
            {              
                return;
            }

            IScriptEditorFactory scriptEditorFactory = Get<IScriptEditorFactory>(); //Singleton
            scriptEditorFactory.Initialize(fileSystem.ScriptsDirectory, globalsType, fileSystem.GetAssemblyReferences());
        }

        public void OnDataModelUpdated(IFileSystem fileSystem, object previousModelInstance, object newDataModelInstance)
        {            
            IScriptEngine scriptEngine = Get<IScriptEngine>();
            IArgumentProcessor argumentProcessor = Get<IArgumentProcessor>();
            IScriptEditorFactory scriptEditorFactory = Get<IScriptEditorFactory>();
        
            if(previousModelInstance == null && newDataModelInstance == null)
            {
                //Is this a valid scenarior ?
                return;
            }

            if(previousModelInstance == null && newDataModelInstance != null)
            {
                argumentProcessor.SetGlobals(newDataModelInstance);                
                var dataModelAssembly = newDataModelInstance.GetType().Assembly;
                scriptEngine.WithAdditionalAssemblyReferences(dataModelAssembly);

                Dispatcher dispatcher = System.Windows.Application.Current.Dispatcher;
                if (dispatcher.CheckAccess())
                {
                    scriptEditorFactory.Initialize(fileSystem.ScriptsDirectory, newDataModelInstance.GetType(), fileSystem.GetAssemblyReferences());

                }
            }

            if (previousModelInstance != null && newDataModelInstance != null)
            {
                argumentProcessor.SetGlobals(newDataModelInstance);              
                var previousDataModelAssembly = previousModelInstance.GetType().Assembly;
                var newDataModleAssembly = newDataModelInstance.GetType().Assembly;               
                scriptEngine.RemoveReferences(previousDataModelAssembly);
                scriptEngine.WithAdditionalAssemblyReferences(newDataModleAssembly);
                Dispatcher dispatcher = System.Windows.Application.Current.Dispatcher;
                if (dispatcher.CheckAccess())
                {
                    scriptEditorFactory.Initialize(fileSystem.ScriptsDirectory, newDataModelInstance.GetType(), fileSystem.GetAssemblyReferences());
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if(isDisposing)
            {
                this.kernel?.Dispose();
            }
        }

        public object Clone()
        {
            return new ServiceResolver(new ChildKernel(this.kernel, new ScopedModules(), new ScriptingModule()));
        }
    }
}
