using Ninject;
using Ninject.Extensions.ChildKernel;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Test.Runner.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Pixel.Automation.Test.Runner
{
    public class ServiceResolver : IServiceResolver
    {
        private readonly IKernel kernel = default;

        public ServiceResolver(IKernel parentKernel)
        {
            //we want these dependencies retrieved from child kernel each time so that prefabs get their own instance.
            this.kernel = new ChildKernel(parentKernel, new ScopedModules(), new ScriptingModule());
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
            scriptEngine.WithSearchPaths(System.Environment.CurrentDirectory, Path.Combine(System.Environment.CurrentDirectory, ""));
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
            if (scriptArguments != null)
            {
                argumentProcessor.SetGlobals(scriptArguments.GetModelData());
            }          
        }       
       
        public void OnDataModelUpdated(IFileSystem fileSystem, ScriptArguments previousArgs, ScriptArguments newArgs)
        {
            IScriptEngine scriptEngine = Get<IScriptEngine>();
           
            if (previousArgs == null && newArgs == null)
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
           
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                this.kernel?.Dispose();
            }
        }

        public object Clone()
        {
            return new ServiceResolver(new ChildKernel(this.kernel, new ScriptingModule(), new DevicesModule()));
        }
    }
}
