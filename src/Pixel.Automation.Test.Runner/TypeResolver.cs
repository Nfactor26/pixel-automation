using Dawn;
using Ninject;
using Spectre.Console.Cli;
using System;

namespace Pixel.Automation.Test.Runner
{
    /// <summary>
    /// Default implementation of <see cref="ITypeResolver"/>
    /// </summary>
    public class TypeResolver : ITypeResolver, IDisposable
    {
        private readonly StandardKernel kernel;
    
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="kernel"></param>
        public TypeResolver(StandardKernel kernel)
        {
            this.kernel = Guard.Argument(kernel).NotNull();
        }

        /// <inheritdoc/>
        public object? Resolve(Type? type)
        {
            if(type != null)
            {
                return kernel.Get(type);
            }
            return null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            //GC.SuppressFinalize(this);

            //if (kernel is IDisposable disposable)
            //{
            //    disposable.Dispose();
            //}
        }
    }
}
