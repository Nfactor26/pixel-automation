using Microsoft.CodeAnalysis;

namespace Pixel.Scripting.Editor.Services.CSharp
{
    public static class TaggedTextExtensions
    {
        public static Core.Models.SymbolDisplayPartKind ToSymbolDisplayPartKind(this TaggedText taggedText)
        {
            switch(taggedText.Tag)
            {
                case TextTags.Alias:
                    return Core.Models.SymbolDisplayPartKind.AliasName;
                case TextTags.AnonymousTypeIndicator:
                    return Core.Models.SymbolDisplayPartKind.AnonymousTypeIndicator;
                case TextTags.Assembly:
                    return Core.Models.SymbolDisplayPartKind.AssemblyName;
                case TextTags.Class:
                    return Core.Models.SymbolDisplayPartKind.ClassName;
                case TextTags.Constant:
                    return Core.Models.SymbolDisplayPartKind.ConstantName;
                case TextTags.Delegate:
                    return Core.Models.SymbolDisplayPartKind.DelegateName;
                case TextTags.Enum:
                    return Core.Models.SymbolDisplayPartKind.EnumName;
                case TextTags.EnumMember:
                    return Core.Models.SymbolDisplayPartKind.EnumMemberName;
                case TextTags.ErrorType:
                    return Core.Models.SymbolDisplayPartKind.ErrorTypeName;
                case TextTags.Event:
                    return Core.Models.SymbolDisplayPartKind.EventName;
                case TextTags.ExtensionMethod:
                    return Core.Models.SymbolDisplayPartKind.ExtensionMethodName;
                case TextTags.Field:
                    return Core.Models.SymbolDisplayPartKind.FieldName;
                case TextTags.Interface:
                    return Core.Models.SymbolDisplayPartKind.InterfaceName;
                case TextTags.Keyword:
                    return Core.Models.SymbolDisplayPartKind.Keyword;
                case TextTags.Label:
                    return Core.Models.SymbolDisplayPartKind.LabelName;
                case TextTags.LineBreak:
                    return Core.Models.SymbolDisplayPartKind.LineBreak;
                case TextTags.Local:
                    return Core.Models.SymbolDisplayPartKind.LocalName;
                case TextTags.Method:
                    return Core.Models.SymbolDisplayPartKind.MethodName;
                case TextTags.Module:
                    return Core.Models.SymbolDisplayPartKind.ModuleName;
                case TextTags.Namespace:
                    return Core.Models.SymbolDisplayPartKind.NamespaceName;
                case TextTags.NumericLiteral:
                    return Core.Models.SymbolDisplayPartKind.NumericLiteral;
                case TextTags.Operator:
                    return Core.Models.SymbolDisplayPartKind.Operator;
                case TextTags.Parameter:
                    return Core.Models.SymbolDisplayPartKind.ParameterName;
                case TextTags.Property:
                    return Core.Models.SymbolDisplayPartKind.PropertyName;
                case TextTags.Punctuation:
                    return Core.Models.SymbolDisplayPartKind.Punctuation;
                case TextTags.RangeVariable:
                    return Core.Models.SymbolDisplayPartKind.RangeVariableName;
                case TextTags.Space:
                    return Core.Models.SymbolDisplayPartKind.Space;
                case TextTags.StringLiteral:
                    return Core.Models.SymbolDisplayPartKind.StringLiteral;
                case TextTags.Struct:
                    return Core.Models.SymbolDisplayPartKind.StructName;
                case TextTags.Text:
                    return Core.Models.SymbolDisplayPartKind.Text;
                case TextTags.TypeParameter:
                    return Core.Models.SymbolDisplayPartKind.TypeParameterName;
                default:
                    return Core.Models.SymbolDisplayPartKind.Text;
            }

        }
    }
}
