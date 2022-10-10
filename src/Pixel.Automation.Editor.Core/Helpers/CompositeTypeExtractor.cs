using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Editor.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pixel.Automation.Editor.Core.Helpers
{
    public class CompositeTypeExtractor : ICompositeTypeExtractor
    {        
    
        public IEnumerable<Type> GetCompositeTypes(Type ownerType)
        {
            return ProcessType(ownerType, ownerType.Assembly, new List<Type>());
        }

        private IEnumerable<Type> ProcessType(Type targetType, Assembly containingAssembly, List<Type> discoveredTypes)
        {
            if (targetType.Assembly.Equals(containingAssembly))
            {
                if (!discoveredTypes.Contains(targetType))
                {
                    discoveredTypes.Add(targetType);
                    foreach (var property in targetType.GetProperties())
                    {
                        foreach (var compositeType in ProcessType(property.PropertyType, containingAssembly, discoveredTypes))
                        {
                            if (!discoveredTypes.Contains(compositeType))
                            {
                                discoveredTypes.Add(compositeType);
                            }
                        }
                    }
                }                     
            }

            if (targetType.IsGenericType)
            {
                foreach (var argument in targetType.GetGenericArguments())
                {
                    if(!discoveredTypes.Contains(argument))
                    {
                        ProcessType(argument, containingAssembly, discoveredTypes);
                    }
                }
            }

            return discoveredTypes;
        }

    }
}
