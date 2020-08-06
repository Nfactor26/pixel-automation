using Ninject;
using Ninject.Extensions.ChildKernel;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Designer.ViewModels.Modules;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Pixel.Automation.Designer.ViewModels
{
    /// <summary>
    /// Service Resolver wraps ninject child kernel to resolve any services.
    /// Ninject child kernel will delegate any service request it can't handle to parent kernel.
    /// The idea is to isolate all dependencies of a Automation Project in to child kernel so that they can be configured for different lifespan such as singleton when required.
    /// Constants can be registered as default as well so that you get same instance throughout the project.
    /// </summary>
    public class ServiceResolver : IServiceResolver
    {
        private readonly IKernel kernel = default;

        public ServiceResolver(IKernel parentKernel)
        {
            this.kernel = new ChildKernel(parentKernel, new WorkspaceModules(), new ScopedModules(), new ScriptingModules(), new DevicesModules());            
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
            return instance ?? throw new Exception(string.Format("Could not locate any instances of contract {0}.", typeof(T).ToString()));
        }


        /// <summary>
        /// Get all services of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetAll<T>()
        {
            var instances = kernel.GetAll<T>();
            return instances ?? throw new Exception(string.Format("Could not locate any instances of contract {0}.", typeof(T).ToString()));
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

        public void ConfigureDefaultServices(IFileSystem fileSystem, ScriptArguments scriptArguments)
        {
            ConfigureScriptEngine(fileSystem, scriptArguments);
            ConfigureArgumentProcessor(scriptArguments);
            ConfigureScriptEditor(fileSystem, scriptArguments);
            ConfigureArgumentTypeProvider(scriptArguments);
        }


        /// <summary>
        /// Configure Script Engine. Script engines will have to be configured for primary as well as secondary entity manager.
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="previousModel"></param>
        /// <param name="newModel"></param>
        private void ConfigureScriptEngine(IFileSystem fileSystem, ScriptArguments scriptArguments)
        {
            IScriptEngineFactory scriptEngineFactory = Get<IScriptEngineFactory>(); //Transient
            IScriptEngine scriptEngine = scriptEngineFactory.CreateScriptEngine(false);
            scriptEngine.SetWorkingDirectory(fileSystem.WorkingDirectory);
            scriptEngine.SetGlobals(scriptArguments.GetModelData());
            scriptEngine.WithSearchPaths(Environment.CurrentDirectory, fileSystem.ReferencesDirectory);
            scriptEngine.WithAdditionalAssemblyReferences(fileSystem.GetAssemblyReferences());
            scriptEngine.WithAdditionalAssemblyReferences(scriptArguments.GetModelType().Assembly);          
            this.RegisterDefault<IScriptEngine>(scriptEngine);            
        }

        /// <summary>
        /// Configure Argument processor. Argument Processor uses a script engine. Script engine must be hence configured before arugment 
        /// processor is configured.
        /// </summary>
        /// <param name="registerAsDefault"></param>
        private void ConfigureArgumentProcessor(ScriptArguments scriptArguments)
        {
            IArgumentProcessor argumentProcessor = Get<IArgumentProcessor>();
            if(scriptArguments != null)
            {
                argumentProcessor.SetGlobals(scriptArguments.GetModelData());
            }
            //this.RegisterDefault<IArgumentProcessor>(argumentProcessor);
        }

        /// <summary>
        /// Add data model assembly to arguments type provider so that it can show types defined in data model assembly 
        /// </summary>
        /// <param name="scriptArguments"></param>
        private void ConfigureArgumentTypeProvider(ScriptArguments scriptArguments)
        {
            IArgumentTypeProvider argumentTypeProvider = Get<IArgumentTypeProvider>();
            argumentTypeProvider.WithDataModelAssembly(scriptArguments.GetModelType().Assembly);
        }     

        /// <summary>
        /// Configure ScriptEditor with specified globalsType. This method can be called again with a different globalsType.
        /// On doing so,  underlying workspace will be disposed and new workspace will be created to reflect change in globalsType.
        /// If there are any inline script editor controls, they are not impacted. They will pick up this change in globalsType.
        /// ScriptEditor should be configured for primary as well as secondary entity manager since they have different globalsType.
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="globalsType"></param>
        private void ConfigureScriptEditor(IFileSystem fileSystem, ScriptArguments scriptArguments)
        {
            Dispatcher dispatcher = System.Windows.Application.Current.Dispatcher;
            if (!dispatcher.CheckAccess())
            {              
                return;
            }

            IScriptEditorFactory scriptEditorFactory = Get<IScriptEditorFactory>(); //Singleton
            var assemblyReferences = new List<string>(fileSystem.GetAssemblyReferences());
            assemblyReferences.Add(scriptArguments.GetModelType().Assembly.Location);
            scriptEditorFactory.Initialize(fileSystem.WorkingDirectory, scriptArguments.GetModelType(), assemblyReferences.ToArray());
            scriptEditorFactory.GetWorkspaceManager().WithSearchPaths(fileSystem.ReferencesDirectory);
        }

        public void OnDataModelUpdated(IFileSystem fileSystem, ScriptArguments previousArgs, ScriptArguments newArgs)
        {            
            IScriptEngine scriptEngine = Get<IScriptEngine>();         
            IScriptEditorFactory scriptEditorFactory = Get<IScriptEditorFactory>();
        
            if(previousArgs == null && newArgs == null)
            {
                //Is this a valid scenario ?
                return;
            }
      
            ConfigureArgumentProcessor(newArgs);

            if (previousArgs == null && newArgs != null)
            {
              
                var dataModelAssembly = newArgs.GetModelType().Assembly;
                scriptEngine.WithAdditionalAssemblyReferences(dataModelAssembly);
                scriptEngine.SetGlobals(newArgs.GetModelData());                          
            }

            if (previousArgs != null && newArgs != null)
            {              
                var previousDataModelAssembly = previousArgs.GetModelType().Assembly;
                var newDataModleAssembly = newArgs.GetModelType().Assembly;               
                scriptEngine.RemoveReferences(previousDataModelAssembly);
                scriptEngine.WithAdditionalAssemblyReferences(newDataModleAssembly);
                scriptEngine.SetGlobals(newArgs.GetModelData());
              
            }
            ConfigureScriptEditor(fileSystem, newArgs);
            ConfigureArgumentTypeProvider(newArgs);
        }

        public void Dispose()
        {
            Dispose(true);          
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
            return new ServiceResolver(new ChildKernel(this.kernel, new ScopedModules(), new ScriptingModules()));
        }
    }
}
