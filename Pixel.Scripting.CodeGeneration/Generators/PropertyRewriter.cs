using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pixel.Scripting.CodeGeneration
{
    internal class PropertyRewriter : CSharpSyntaxRewriter
    {
        public PropertyDefinition PropertyDefinition { get; set; } = new PropertyDefinition();

        TypeSyntaxGenerator typeSyntaxGenerator;

        public PropertyRewriter(TypeSyntaxGenerator typeSyntaxGenerator)
        {
            this.typeSyntaxGenerator = typeSyntaxGenerator;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var propertyDeclaration = SyntaxFactory.PropertyDeclaration(typeSyntaxGenerator.Create(PropertyDefinition.PropertyType),
                PropertyDefinition.PropertyName)
                 .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                 .AddAccessorListAccessors(
                     SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                     SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                     );

            if(!string.IsNullOrEmpty(PropertyDefinition.DefaultValue))
            {
              var initializer =  SyntaxFactory.EqualsValueClause(SyntaxFactory.Token(SyntaxKind.EqualsToken),
                    SyntaxFactory.ParseExpression(PropertyDefinition.DefaultValue.ToString()));

                propertyDeclaration = propertyDeclaration.WithInitializer(initializer);
                propertyDeclaration = propertyDeclaration.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            }            
           
            return node.AddMembers(propertyDeclaration);
        }        

    }
}
