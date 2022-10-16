using Dawn;
using Ninject;
using Spectre.Console.Cli;
using System;

namespace Pixel.Automation.Test.Runner;

/// <summary>
/// Default implementation of <see cref="ITypeRegistrar"/>
/// </summary>
public class TypeRegistrar : ITypeRegistrar
{
    private readonly StandardKernel kernel;
   
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="kernel"></param>
    public TypeRegistrar(StandardKernel kernel)
    {
        this.kernel = Guard.Argument(kernel).NotNull();
    }

    /// <inheritdoc/>    
    public ITypeResolver Build()
    {
        return new TypeResolver(kernel);
    }

    /// <inheritdoc/>    
    public void Register(Type service, Type implementation)
    {
        Guard.Argument(service).NotNull();
        Guard.Argument(implementation).NotNull();
        kernel.Bind(service).To(implementation);
    }

    /// <inheritdoc/>    
    public void RegisterInstance(Type service, object implementation)
    {
        Guard.Argument(service).NotNull();
        Guard.Argument(implementation).NotNull();
        kernel.Bind(service).ToConstant(implementation);
    }

    /// <inheritdoc/>    
    public void RegisterLazy(Type service, Func<object> factoryMethod)
    {
        Guard.Argument(service).NotNull();
        Guard.Argument(factoryMethod).NotNull();
        kernel.Bind(service).ToMethod((context) => factoryMethod());
    }
}
