using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Pixel.Scripting.CodeGeneration
{
    public class TypeSyntaxGenerator
    {      
        public TypeSyntax Create(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

           return SyntaxFactory.ParseTypeName(GetDisplayName(type));
        }

        public string GetDisplayName(Type type)
        {
            switch (type.IsGenericType)
            {
                case true:
                    return type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(x => GetDisplayName(x)).ToArray()) + ">";
                case false:
                    return type.Name;
            }           
        }

    }
}
