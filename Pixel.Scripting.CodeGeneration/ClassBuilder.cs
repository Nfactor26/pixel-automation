using Dawn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Pixel.Scripting.CodeGeneration.Generators;
using Pixel.Automation.Core.Interfaces.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixel.Scripting.CodeGeneration
{
    internal class ClassBuilder : IClassGenerator
    {
        SyntaxNode classRoot;

        public ClassBuilder(string className, string nameSpace, IEnumerable<string> imports)
        {
            classRoot = ClassGenerator.CreateClass(new ClassDefinition() { ClassName = className, NameSpace = nameSpace, Imports = imports });
        }

        public IClassGenerator AddProperty(string propertyName, Type propertyType, string defaultValue = "")
        {
            Guard.Argument(propertyName).NotNull().NotEmpty();
            Guard.Argument(propertyType).NotNull();
            Guard.Argument(defaultValue).NotNull();

            PropertyRewriter propertyGenerator = new PropertyRewriter(new TypeSyntaxGenerator());
            propertyGenerator.PropertyDefinition = new PropertyDefinition() { PropertyName = propertyName,
                PropertyType = propertyType, DefaultValue = defaultValue };
            classRoot = propertyGenerator.Visit(classRoot);
            return this;
        }


        public IClassGenerator AddAttribute(string targetProperty, Type attributeType, IEnumerable<KeyValuePair<string, object>> attributeArguments)
        {
            Guard.Argument(targetProperty).NotNull().NotEmpty();
            Guard.Argument(attributeType).NotNull();
            Guard.Argument(attributeArguments).NotNull();

            AttributeRewriter attributeRewriter = new AttributeRewriter() { AttributeDefinition = new AttributeDefinition() { TargetProperty = targetProperty, AttributeType = attributeType, AttributeParameters = attributeArguments } }; 
            classRoot = attributeRewriter.Visit(classRoot);

            return this;
        }

        public IClassGenerator SetBaseClass(Type baseClassType)
        {
            Guard.Argument(baseClassType).NotNull();

            BaseTypeRewriter basetypeRewriter = new BaseTypeRewriter() { InheritFrom = baseClassType };
            classRoot = basetypeRewriter.Visit(classRoot);

            return this;
        }
      
        public string GetGeneratedCode()
        {
            var workSpace = new AdhocWorkspace();

            OptionSet options = workSpace.Options;
            options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInProperties, true);
            options = options.WithChangedOption(CSharpFormattingOptions.WrappingKeepStatementsOnSingleLine, true);
            options = options.WithChangedOption(CSharpFormattingOptions.NewLineForMembersInObjectInit, false);
            //options = options.WithChangedOption(FormattingOptions.AllowDisjointSpanMerging, true);
            //options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAccessors, false);

            //options = options.WithChangedOption(CSharpFormattingOptions.WrappingPreserveSingleLine, false);
            SyntaxNode formattedNode = Formatter.Format(classRoot, workSpace, options);
        
            return formattedNode.NormalizeWhitespace().ToFullString();
        }

        public bool HasErrors(out string errorDetails)
        {          
            StringBuilder errorMessages = new StringBuilder();
            if(classRoot != null)
            {
                var diagnostics = classRoot.GetDiagnostics();
                if(diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                {
                    foreach(var diagnostic in diagnostics)
                    {
                        if(diagnostic.Severity == DiagnosticSeverity.Error)
                        {
                            errorMessages.AppendLine(diagnostic.GetMessage());                            
                        }
                    }
                    errorDetails = errorMessages.ToString();
                    return true;
                }                
            }
            errorDetails = string.Empty;
            return false;
        }
    }
}
