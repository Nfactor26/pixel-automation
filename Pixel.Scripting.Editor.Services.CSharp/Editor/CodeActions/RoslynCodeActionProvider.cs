using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Reflection;

namespace Pixel.Scripting.Editor.Services.CodeActions
{
    public class RoslynCodeActionProvider : AbstractCodeActionProvider
    {
        private static readonly ImmutableArray<Assembly> DefaultCodeActionProviders =
            ImmutableArray.Create
            (
                 // Microsoft.CodeAnalysis.Features
                 typeof(FeaturesResources).GetTypeInfo().Assembly,
                 // Microsoft.CodeAnalysis.CSharp.Features
                 typeof(CSharpFeaturesResources).GetTypeInfo().Assembly
            );
        public RoslynCodeActionProvider()  : base("Roslyn", DefaultCodeActionProviders)
        {
        }
    }
}
