using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pixel.Automation.Core.Interfaces
{
    /// <summary>
    /// Type provider maintains known component types which are serializable.
    /// Serializer needs to know the these derived types to serialize/desirialize them correctly.
    /// </summary>
    public interface ITypeProvider
    {
        /// <summary>
        /// Get all the known types
        /// </summary>
        /// <returns></returns>
        List<Type> GetKnownTypes();       

        /// Find and load types from specified assembly
        /// </summary>
        /// <param name="assembly"></param>
        void LoadTypesFromAssembly(Assembly assembly);
    }
}
