using Pixel.Scripting.Common.CSharp;
using System.Composition;

namespace Pixel.Scripting.Editor.Services.CodeActions
{
    [Shared]
    [Export(typeof(ICodeActionProvider))]
    public class RoslynCodeActionProvider : AbstractCodeActionProvider
    {
        [ImportingConstructor]
        public RoslynCodeActionProvider(IHostServicesProvider featuresHostServicesProvider)
            : base("Roslyn", featuresHostServicesProvider.Assemblies)
        {
        }
    }
}
