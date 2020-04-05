using System;
using System.Linq;

namespace Pixel.Automation.Core.Extensions
{
    public static class TypeExtensions
    {
        public static string GetDisplayName(this Type type)
        {
            switch (type.IsGenericType)
            {
                case true:
                    if (type.ContainsGenericParameters)
                        return type.Name.Split('`')[0];
                    else
                        return type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(x => GetDisplayName(x)).ToArray()) + ">";
                case false:
                    return type.Name;
            }          
        }
    }
}
