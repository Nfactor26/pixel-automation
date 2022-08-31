using Dawn;
using Pixel.Automation.Editor.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pixel.Automation.Editor.Core.TypeBrowser
{
    /// <summary>
    /// Implementation of <see cref="IArgumentTypeProvider"/> with a fix collection of types passed during initialization.
    /// </summary>
    internal class SimpleArgumentTypeProvider : IArgumentTypeProvider
    {
        private readonly List<TypeDefinition> knownTypes = new List<TypeDefinition>();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="types"></param>
        public SimpleArgumentTypeProvider(IEnumerable<Type> types)
        {
            Guard.Argument(types).NotNull().NotEmpty();
            foreach (var type in types)
            {
                this.knownTypes.Add(new TypeDefinition(type));
            }
        }

        /// <inheritdoc/>       
        public IEnumerable<TypeDefinition> GetAllKnownTypes()
        {
            return this.knownTypes;
        }

        /// <inheritdoc/>      
        public IEnumerable<TypeDefinition> GetCommonTypes()
        {
            return Enumerable.Empty<TypeDefinition>();
        }

        /// <inheritdoc/>      
        public IEnumerable<TypeDefinition> GetCustomDefinedTypes()
        {
            return this.knownTypes;
        }

        /// <inheritdoc/>      
        public IArgumentTypeProvider WithAdditionalAssemblyPaths(params string[] assemblyPaths)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>      
        public IArgumentTypeProvider WithDataModelAssembly(Assembly assembly)
        {
            throw new NotImplementedException();
        }
    }
}
