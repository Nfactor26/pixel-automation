using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Pixel.Script.Editor.Services.CSharp.Helpers
{
    public static class SymbolExtensions
    {
        private readonly static CachedStringBuilder s_cachedBuilder;

        public static string GetMetadataName(this ISymbol symbol)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            var symbols = new Stack<ISymbol>();

            while (symbol != null)
            {
                if (symbol.Kind == SymbolKind.Assembly ||
                    symbol.Kind == SymbolKind.NetModule)
                {
                    break;
                }

                if ((symbol as INamespaceSymbol)?.IsGlobalNamespace == true)
                {
                    break;
                }

                symbols.Push(symbol);
                symbol = symbol.ContainingSymbol;
            }

            var builder = s_cachedBuilder.Acquire();
            try
            {
                ISymbol current = null, previous = null;

                while (symbols.Count > 0)
                {
                    current = symbols.Pop();

                    if (previous != null)
                    {
                        if (previous.Kind == SymbolKind.NamedType &&
                            current.Kind == SymbolKind.NamedType)
                        {
                            builder.Append('+');
                        }
                        else
                        {
                            builder.Append('.');
                        }
                    }

                    builder.Append(current.MetadataName);

                    previous = current;
                }

                return builder.ToString();
            }
            finally
            {
                s_cachedBuilder.Release(builder);
            }
        }

        public static string GetAccessibilityString(this ISymbol symbol)
        {
            switch (symbol.DeclaredAccessibility)
            {
                case Accessibility.Public:
                    return SymbolAccessibilities.Public;
                case Accessibility.Internal:
                    return SymbolAccessibilities.Internal;
                case Accessibility.Private:
                    return SymbolAccessibilities.Private;
                case Accessibility.Protected:
                    return SymbolAccessibilities.Protected;
                case Accessibility.ProtectedOrInternal:
                    return SymbolAccessibilities.ProtectedInternal;
                case Accessibility.ProtectedAndInternal:
                    return SymbolAccessibilities.PrivateProtected;
                default:
                    return null;
            }
        }

        public static string GetKindString(this ISymbol symbol)
        {
            switch (symbol)
            {
                case INamespaceSymbol _:
                    return SymbolKinds.Namespace;
                case INamedTypeSymbol namedTypeSymbol:
                    return namedTypeSymbol.GetKindString();
                case IMethodSymbol methodSymbol:
                    return methodSymbol.GetKindString();
                case IFieldSymbol fieldSymbol:
                    return fieldSymbol.GetKindString();
                case IPropertySymbol propertySymbol:
                    return propertySymbol.GetKindString();
                case IEventSymbol _:
                    return SymbolKinds.Event;
                default:
                    return SymbolKinds.Unknown;
            }
        }

        public static string GetKindString(this INamedTypeSymbol namedTypeSymbol)
        {
            switch (namedTypeSymbol.TypeKind)
            {
                case TypeKind.Class:
                    return SymbolKinds.Class;
                case TypeKind.Delegate:
                    return SymbolKinds.Delegate;
                case TypeKind.Enum:
                    return SymbolKinds.Enum;
                case TypeKind.Interface:
                    return SymbolKinds.Interface;
                case TypeKind.Struct:
                    return SymbolKinds.Struct;
                default:
                    return SymbolKinds.Unknown;
            }
        }

        public static string GetKindString(this IMethodSymbol methodSymbol)
        {
            switch (methodSymbol.MethodKind)
            {
                case MethodKind.Ordinary:
                case MethodKind.ReducedExtension:
                case MethodKind.ExplicitInterfaceImplementation:
                    return SymbolKinds.Method;
                case MethodKind.Constructor:
                case MethodKind.StaticConstructor:
                    return SymbolKinds.Constructor;
                case MethodKind.Destructor:
                    return SymbolKinds.Destructor;
                case MethodKind.Conversion:
                case MethodKind.BuiltinOperator:
                case MethodKind.UserDefinedOperator:
                    return SymbolKinds.Operator;
                default:
                    return SymbolKinds.Unknown;
            }
        }

        public static string GetKindString(this IFieldSymbol fieldSymbol)
        {
            if (fieldSymbol.ContainingType?.TypeKind == TypeKind.Enum &&
                fieldSymbol.HasConstantValue)
            {
                return SymbolKinds.EnumMember;
            }

            return fieldSymbol.IsConst
                ? SymbolKinds.Constant
                : SymbolKinds.Field;
        }

        public static string GetKindString(this IPropertySymbol propertySymbol)
        {
            return propertySymbol.IsIndexer
                ? SymbolKinds.Indexer
                : SymbolKinds.Property;
        }

        public static bool IsOverridable(this ISymbol symbol) => symbol?.ContainingType?.TypeKind == TypeKind.Class && !symbol.IsSealed && (symbol.IsVirtual || symbol.IsAbstract || symbol.IsOverride);

        /// <summary>
        /// Do not use this API in new OmniSharp endpoints. Use <see cref="GetKindString(ISymbol)"/> instead.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static string GetKind(this ISymbol symbol)
        {
            if (symbol is INamedTypeSymbol namedType)
            {
                return Enum.GetName(namedType.TypeKind.GetType(), namedType.TypeKind);
            }

            if (symbol.Kind == SymbolKind.Field &&
                symbol.ContainingType?.TypeKind == TypeKind.Enum &&
                symbol.Name != WellKnownMemberNames.EnumBackingFieldName)
            {
                return "EnumMember";
            }

            if ((symbol as IFieldSymbol)?.IsConst == true)
            {
                return "Const";
            }

            return Enum.GetName(symbol.Kind.GetType(), symbol.Kind);
        }

        internal static INamedTypeSymbol GetTopLevelContainingNamedType(this ISymbol symbol)
        {
            // Traverse up until we find a named type that is parented by the namespace
            var topLevelNamedType = symbol;
            while (topLevelNamedType.ContainingSymbol != symbol.ContainingNamespace ||
                topLevelNamedType.Kind != SymbolKind.NamedType)
            {
                topLevelNamedType = topLevelNamedType.ContainingSymbol;
            }

            return (INamedTypeSymbol)topLevelNamedType;
        }
    }
}
