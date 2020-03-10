using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace Pixel.Scripting.CodeGeneration
{
    internal class ClassGenerator
    {
        
        public static CSharpSyntaxNode CreateClass(ClassDefinition classDefinition)
        {

            var syntaxFactory = SyntaxFactory.CompilationUnit();
          
            var distinctImports = classDefinition.Imports.Distinct();
            foreach(var import in distinctImports)
            {
                syntaxFactory = syntaxFactory.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(import)));
            }

        
            //  Create a class: (class Order)
            var classDeclaration = SyntaxFactory.ClassDeclaration(classDefinition.ClassName);

            // Add the public modifier: (public class Order)
            classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            if(!string.IsNullOrEmpty(classDefinition.NameSpace))
            {
                // Create a namespace: (namespace CodeGenerationSample)
                var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(classDefinition.NameSpace)).NormalizeWhitespace();
                @namespace = @namespace.AddMembers(classDeclaration);
                syntaxFactory = syntaxFactory.AddMembers(@namespace);
            }
            else
            {
                syntaxFactory = syntaxFactory.AddMembers(classDeclaration);
            }
            return syntaxFactory;
        }
      

    }
}
