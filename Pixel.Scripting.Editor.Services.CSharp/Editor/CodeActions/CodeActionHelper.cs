using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Pixel.Script.Editor.Services.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pixel.Scripting.Editor.Services.CodeActions
{
    public class CodeActionHelper
    {
        public const string AddImportProviderName = "Microsoft.CodeAnalysis.CSharp.AddImport.CSharpAddImportCodeFixProvider";
        public const string RemoveUnnecessaryUsingsProviderName = "Microsoft.CodeAnalysis.CSharp.RemoveUnnecessaryImports.CSharpRemoveUnnecessaryImportsCodeFixProvider";

        private static readonly HashSet<string> _roslynListToRemove = new HashSet<string>
        {
            "Microsoft.CodeAnalysis.CSharp.AddMissingReference.CSharpAddMissingReferenceCodeFixProvider",
            "Microsoft.CodeAnalysis.CSharp.CodeFixes.Async.CSharpConvertToAsyncMethodCodeFixProvider",
            "Microsoft.CodeAnalysis.CSharp.CodeFixes.Iterator.CSharpChangeToIEnumerableCodeFixProvider",
            "Microsoft.CodeAnalysis.ChangeSignature.ChangeSignatureCodeRefactoringProvider",
            "Microsoft.CodeAnalysis.ExtractInterface.ExtractInterfaceCodeRefactoringProvider"
        };

        private static bool s_validated;

        private static void ValidateRoslynList()
        {
            if (s_validated)
            {
                return;
            }
          
            var assemblies = new[]
            {
               typeof(WorkspacesResources).GetTypeInfo().Assembly,
               typeof(FeaturesResources).GetTypeInfo().Assembly,          
               typeof(CSharpFeaturesResources).GetTypeInfo().Assembly              
            };

            var typeNames = _roslynListToRemove.Concat(new[] { AddImportProviderName, RemoveUnnecessaryUsingsProviderName });

            foreach (var typeName in typeNames)
            {
                if (!ExistsInAssemblyList(typeName, assemblies))
                {
                    throw new InvalidOperationException($"Could not find '{typeName}'. Has this type name changed?");
                }
            }

            s_validated = true;
        }

        private static bool ExistsInAssemblyList(string typeName, Assembly[] assemblies)
        {
            return assemblies.Any(a => a.GetType(typeName) == null);
        }
                
        public CodeActionHelper()
        {
            ValidateRoslynList();
        }

        public bool IsDisallowed(string typeName)
        {
            return _roslynListToRemove.Contains(typeName);
        }

        public bool IsDisallowed(CodeFixProvider provider)
        {
            return IsDisallowed(provider.GetType().FullName);
        }

        public bool IsDisallowed(CodeRefactoringProvider provider)
        {
            return IsDisallowed(provider.GetType().FullName);
        }
    }
}
