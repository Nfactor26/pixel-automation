using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pixel.Automation.Core.Interfaces
{
    public interface ITypeProvider
    {      
        Dictionary<string,List<Type>> KnownTypes
        {
            get;

        }

        List<Type> GetAllTypes();       

        void LoadDefaultTypes();

        void RefreshDefaultTypeCache();

        void ClearCustomTypes();

        void LoadTypesFromAssembly(List<string> loadFromAssemblies);

        void LoadTypesFromAssembly(Assembly assembly);
    }
}
