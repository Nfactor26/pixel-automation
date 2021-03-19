using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Scripting.CodeGeneration.Generators
{
    public class AttributeRewriter : CSharpSyntaxRewriter
    {
        public AttributeDefinition AttributeDefinition { get; set; } = new AttributeDefinition();
        public AttributeRewriter()
        {
           
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var propertyNodes = node.DescendantNodes().OfType<PropertyDeclarationSyntax>();
            var targetProperty = propertyNodes.FirstOrDefault(p => p.Identifier.Text.Equals(AttributeDefinition.TargetProperty));
            if(targetProperty != null)
            {
               var targetPropertyWithAttribute = VisitPropertyDeclaration(targetProperty) as PropertyDeclarationSyntax;
                node = node.ReplaceNode(targetProperty, targetPropertyWithAttribute);
            }
            return node;
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if(!node.Identifier.Text.Equals(AttributeDefinition.TargetProperty))
            {
                return node;
            }
        
            var attributeName = AttributeDefinition.AttributeType.Name;
            AttributeSyntax attributeDeclaration = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName));

            var attributeArguments = new List<AttributeArgumentSyntax>();
            foreach(var attributeParameter in AttributeDefinition.AttributeParameters)
            {
                var attributeArgument = SyntaxFactory.AttributeArgument(null, string.IsNullOrEmpty(attributeParameter.Key)? null : SyntaxFactory.NameColon(attributeParameter.Key), SyntaxFactory.ParseExpression(attributeParameter.Value.ToString()));
                attributeArguments.Add(attributeArgument);
            }

            attributeDeclaration = attributeDeclaration.WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(attributeArguments)));
            return node.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new List<AttributeSyntax>() {attributeDeclaration})));
        }      

    }
}
