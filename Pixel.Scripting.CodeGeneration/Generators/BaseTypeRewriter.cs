using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pixel.Automation.Core.Extensions;
using System;

namespace Pixel.Scripting.CodeGeneration.Generators
{
    class BaseTypeRewriter : CSharpSyntaxRewriter
    {
        public Type InheritFrom { get; set; }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            node = node.WithBaseList(
                SyntaxFactory.BaseList(
                    SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                        SyntaxFactory.SimpleBaseType(
                            SyntaxFactory.IdentifierName(InheritFrom.GetDisplayName())))));
            return node;
        }
    }
}
