using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Reflection;

namespace Pixel.Scripting.Editor.Services.CodeActions
{
    public interface ICodeActionProvider
    {
        ImmutableArray<CodeRefactoringProvider> CodeRefactoringProviders { get; }
        ImmutableArray<CodeFixProvider> CodeFixProviders { get; }
        ImmutableArray<DiagnosticAnalyzer> CodeDiagnosticAnalyzerProviders { get; }
        ImmutableArray<Assembly> Assemblies { get; }
    }
}
