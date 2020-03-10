using System.Collections.Generic;
using System.Reflection;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IArgumentTypeProvider
    {
        /// <summary>
        /// Specify a collection of folder which should be looked for assemblies
        /// </summary>
        /// <param name="assemblyPaths"></param>
        /// <returns></returns>
        IArgumentTypeProvider WithAdditionalAssemblyPaths(params string[] assemblyPaths);

        /// <summary>
        /// Add datamodel assembly which should be scanned for any custom defined types
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        IArgumentTypeProvider WithDataModelAssembly(Assembly assembly);

        /// <summary>
        /// Get most commonly used types such as int, string, etc.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TypeDefinition> GetCommonTypes();

        /// <summary>
        /// Get all types defined in all dll's loaded in current app domain or loaded from additional assembly paths
        /// </summary>
        /// <returns></returns>
        IEnumerable<TypeDefinition> GetAllKnownTypes();

        /// <summary>
        /// Get custom types defined in DataModel assembly 
        /// </summary>
        /// <returns></returns>
        IEnumerable<TypeDefinition> GetCustomDefinedTypes();

    }
}
