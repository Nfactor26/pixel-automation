using Ninject;
using Ninject.Extensions.ChildKernel;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Designer.ViewModels.Modules;
using System;
using System.Collections.Generic;

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

        public void Dispose()
        {
            Dispose(true);          
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if(isDisposing)
            {
                foreach (IDisposable module in this.kernel.GetModules())
                {
                    module.Dispose();
                }
                this.kernel?.Dispose();
            }
        }
    }
}
