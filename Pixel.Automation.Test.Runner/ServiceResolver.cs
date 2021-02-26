using Ninject;
using Ninject.Extensions.ChildKernel;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Test.Runner.Modules;
using System;
using System.Collections.Generic;

namespace Pixel.Automation.Test.Runner
{
    public class ServiceResolver : IServiceResolver
    {
        private readonly IKernel kernel = default;

        public ServiceResolver(IKernel parentKernel)
        {
            //we want these dependencies retrieved from child kernel each time so that prefabs get their own instance.
            this.kernel = new ChildKernel(parentKernel, new ScopedModules());
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
            if (isDisposing)
            {
                this.kernel?.Dispose();
            }
        }
    }
}
