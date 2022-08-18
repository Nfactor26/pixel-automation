using Microsoft.CodeAnalysis;
using Pixel.Scripting.Editor.Core;
using System;

namespace Pixel.Scripting.Editor.Services.CSharp.TypeLookup
{
    internal static class SymbolExtensions
    {
        internal static Glyph GetGlyph(this ISymbol symbol)
        {
            Glyph publicIcon;

            switch (symbol.Kind)
            {
                case SymbolKind.Alias:
                    return ((IAliasSymbol)symbol).Target.GetGlyph();

                case SymbolKind.Assembly:
                    return Glyph.Assembly;

                case SymbolKind.ArrayType:
                    return Glyph.ClassPublic;

                case SymbolKind.DynamicType:
                    return Glyph.ClassPublic;

                case SymbolKind.Event:
                    publicIcon = Glyph.EventPublic;
                    break;

                case SymbolKind.Field:
                    var containingType = symbol.ContainingType;
                    if (containingType != null && containingType.TypeKind == TypeKind.Enum)
                    {
                        return Glyph.EnumMemberPublic;
                    }

                    publicIcon = ((IFieldSymbol)symbol).IsConst ? Glyph.ConstantPublic : Glyph.FieldPublic;
                    break;

                case SymbolKind.Label:
                    return Glyph.Label;

                case SymbolKind.Local:
                case SymbolKind.Discard:
                    return Glyph.Local;

                case SymbolKind.NamedType:
                case SymbolKind.ErrorType:
                    {
                        switch (((INamedTypeSymbol)symbol).TypeKind)
                        {
                            case TypeKind.Class:
                                publicIcon = Glyph.ClassPublic;
                                break;

                            case TypeKind.Delegate:
                                publicIcon = Glyph.DelegatePublic;
                                break;

                            case TypeKind.Enum:
                                publicIcon = Glyph.EnumPublic;
                                break;

                            case TypeKind.Interface:
                                publicIcon = Glyph.InterfacePublic;
                                break;

                            case TypeKind.Module:
                                publicIcon = Glyph.ModulePublic;
                                break;

                            case TypeKind.Struct:
                                publicIcon = Glyph.StructurePublic;
                                break;

                            case TypeKind.Error:
                                return Glyph.Error;

                            default:
                                throw new ArgumentException("Symbol doesn't have an icon.", nameof(symbol));
                        }

                        break;
                    }

                case SymbolKind.Method:
                    {
                        var methodSymbol = (IMethodSymbol)symbol;

                        if (methodSymbol.MethodKind is MethodKind.UserDefinedOperator or
                            MethodKind.Conversion or
                            MethodKind.BuiltinOperator)
                        {
                            return Glyph.Operator;
                        }
                        else if (methodSymbol.IsExtensionMethod ||
                                 methodSymbol.MethodKind == MethodKind.ReducedExtension)
                        {
                            publicIcon = Glyph.ExtensionMethodPublic;
                        }
                        else
                        {
                            publicIcon = Glyph.MethodPublic;
                        }
                    }

                    break;

                case SymbolKind.Namespace:
                    return Glyph.Namespace;

                case SymbolKind.NetModule:
                    return Glyph.Assembly;

                case SymbolKind.Parameter:
                    //return symbol.IsImplicitValueParameter()
                    //    ? Glyph.Keyword : Glyph.Parameter;
                      return Glyph.Parameter;

                case SymbolKind.PointerType:
                    return ((IPointerTypeSymbol)symbol).PointedAtType.GetGlyph();

                case SymbolKind.FunctionPointerType:
                    return Glyph.Intrinsic;

                case SymbolKind.Property:
                    {
                        var propertySymbol = (IPropertySymbol)symbol;

                        if (propertySymbol.IsWithEvents)
                        {
                            publicIcon = Glyph.FieldPublic;
                        }
                        else
                        {
                            publicIcon = Glyph.PropertyPublic;
                        }
                    }

                    break;

                case SymbolKind.RangeVariable:
                    return Glyph.RangeVariable;

                case SymbolKind.TypeParameter:
                    return Glyph.TypeParameter;

                default:
                    throw new ArgumentException("Symbol doesn't have an icon.", nameof(symbol));
            }

            switch (symbol.DeclaredAccessibility)
            {
                case Accessibility.Private:
                    publicIcon += Glyph.ClassPrivate - Glyph.ClassPublic;
                    break;

                case Accessibility.Protected:
                case Accessibility.ProtectedAndInternal:
                case Accessibility.ProtectedOrInternal:
                    publicIcon += Glyph.ClassProtected - Glyph.ClassPublic;
                    break;

                case Accessibility.Internal:
                    publicIcon += Glyph.ClassInternal - Glyph.ClassPublic;
                    break;
            }

            return publicIcon;
        }
    }
}
